### Description
SpiderRT lets you search for specific terms across multiple code base repositories, reguardless of their language.

Useful for knowing what projects may be using a certain database table, method in a shared library, etc.

### Future plans include:
* Handle file deletions from Solr when they're deleted from the code repository
* Supporting multiple VCS providers (SVN, Hg, etc)
* Show relevant line from file match and highlight the search term in the web UI
* Ability to save searches that are then exposed via a RESTful URL (so tests you write can get that data and throw up a failing build if unallowed searches crop up again)
* Ability to import VCS roots from at least TeamCity, possibly Jenkins too
* Speed improvements (only re-indexing updated files rather than all of them, etc)
* More complete UI that lets you set VCS roots, Solr server connections, file/directory indexing exclusions, looks prettier, etc.
* Move to an embedded RavenDB instance
* Ensure it can run on Mono
* More advanced search functionality (look ahead/behind, only certain file types, etc)
* Ease deployment story
