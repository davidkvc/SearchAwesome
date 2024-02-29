using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SearchAwesome.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SearchDbContext _context;

        public List<User> Users { get; set; } = new List<User>();

        public IndexModel(ILogger<IndexModel> logger, SearchDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task OnGet()
        {
            Users = await _context.Users.ToListAsync();
        }
    }
}
