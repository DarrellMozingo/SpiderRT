### Description
SpiderRT lets you search for specific terms across multiple code base repositories, regardless of their language.

Useful for knowing what projects may be using a certain database table, method in a shared library, etc.

### Future plans include:
* Proper tests!
* Make the indexing/VCS polling code a service that'll run periodically
* Handle file deletions from Solr when they're deleted from the code repository
* Support multiple VCS providers (SVN, Hg, etc)
* Show relevant line from file match and highlight the search term in the web UI
* Ability to save searches that are then exposed via a RESTful URL (so tests you write can get that data and throw up a failing build if unallowed searches crop up again)
* Ability to import VCS roots from at least TeamCity, possibly Jenkins too
* Speed improvements (only re-indexing updated files rather than all of them, etc.)
* More complete website UI that lets you set VCS roots & file/directory indexing exclusions, looks prettier, etc.
* Provide the option of an embedded RavenDB instance so it doesn't require a separate instance
* Ensure the whole stack can run on Mono
* Index commit messages and make them searchable
* More advanced search functionality (look ahead/behind, only certain file types, etc.)
* Easier deployment story