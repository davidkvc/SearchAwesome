using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SearchAwesome.Pages;

[IgnoreAntiforgeryToken]
public class UserSearchModel : PageModel
{
    private readonly ILogger<UserSearchModel> _logger;
    private readonly SearchDbContext _context;

    public List<User> Users { get; set; } = new List<User>();

    public UserSearchModel(SearchDbContext context, ILogger<UserSearchModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    class SimilarWordsResult
    {
        public string InputWord { get; set; }
        public string[] SimilarWords { get; set; }
    }

    public async Task OnPost([FromForm] string? q)
    {
        _logger.LogInformation("Searching users using '{query}'", q);

        //normalize query
        var cleanTerms = await _context.Database.SqlQuery<string>($"""
                select unnest as "Value"
                from unnest(string_to_array(plainto_tsquery('simple', {q})::text, ' & ')) 
            """).ToListAsync();

        cleanTerms = cleanTerms.Select(x => x.Trim('\'')).ToList();

        //find similar words
        var similarWords = await _context.Database.SqlQuery<string[]>($"""
                select vs.v || array_agg(words.word) as "Value"
                from (
            	    select word
            	    from ts_stat('select ts from users')
                    union
                    select unnest as word
                    from unnest({cleanTerms})
                ) words, unnest({cleanTerms}) vs(v)
                where levenshtein_less_equal(words.word, vs.v, 2) < 2
                group by vs.v
            """).ToListAsync();

        var normalizedQuery = string.Join(" & ", similarWords
            .Select(words => "(" + string.Join(" | ", words.Select(x => $"{x}:*")) + ")"));

        _logger.LogInformation("Using normalized query {Query}", normalizedQuery);

        var baseQuery = string.IsNullOrEmpty(q)
            ? _context.Users
            : _context.Users.FromSql($"""
                select users.*
                from users  
                where ts @@ to_tsquery('simple', {normalizedQuery})
                order by ts_rank(ts, to_tsquery('simple', {normalizedQuery})) desc
            """);
        Users = await baseQuery
            .AsNoTracking()
            .ToListAsync();
    }
}
