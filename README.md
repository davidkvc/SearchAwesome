# SearchAwesome

This is an exploration of how could we use Postgres' full text search capabilities to
search user accounts

## Setup

```sql
create extension unaccent;
create extension fuzzystrmatch;

create table users (
	id int generated always as identity primary key,
	ma_id text,
	da_id text,
	name text,
	display_name text,
	email text,
	ts tsvector
);

CREATE or replace FUNCTION users_trigger() RETURNS trigger AS $$
declare
	email_domain text;
begin
	email_domain := SUBSTRING(new.email, position('@' in new.email));
 	new.ts :=
  		setweight(to_tsvector('simple', coalesce(new.name, '')), 'A') ||
		setweight(to_tsvector('simple', unaccent(coalesce(new.name, ''))), 'B') ||
  		setweight(to_tsvector('simple', unaccent(coalesce(new.display_name, ''))), 'B') ||
	  	setweight(to_tsvector('simple', coalesce(email_domain, '')), 'B') ||
		setweight(to_tsvector('simple', coalesce(new.email, '')), 'C');
	return new;
end
$$ LANGUAGE plpgsql;

CREATE TRIGGER users_tsvectorupdate BEFORE INSERT OR UPDATE
    ON users FOR EACH ROW EXECUTE FUNCTION users_trigger();
   
CREATE INDEX users_ts_idx ON users USING gin(ts);
```