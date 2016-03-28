Sprocket is an ongoing project, a long time in the making and likely to continue being made for a long time.

# Current Status #

The current release version is **0.4**. This is essentially a web framework, providing time-saving tools and methods for putting together a database-driven website with a static set of pages (editable if you write editing functionality).

Pages and templates are defined and pointed to in XML files. Templates are HTML with embedded SprocketScript syntax where desired. Templates have full inheritance capability, so you can define master templates, subpage templates, and still deeper templates beyond that.

A security model is provided and is the primary piece of functionality available within the admin system. Users, roles, permissions and permission types are the focus of the security system. Permissions can be applied to both roles and individual users, and roles can inherit the permissions of other roles. The user interface makes use of AJAX for fast access.

# Release 0.5 #

Release **0.5** is in progress and implements the following:

  * A database-driven page editing user interface with a full versioning/revision system for content. You should be able to roll content back to any previous state in its history. Pages can be saved as drafts. Deleted pages can be restored.
  * Standard data provider/layer base classes now exist to accelerate development of new database-driven modules.
  * The admin system has received a make-over and now looks a lot cleaner. It also now makes full use of SprocketScript and the base page content system (defined in XML) to make customisation and extension of the admin system very easy.
  * SQLite is now largely supported as the default database provider. Therefore Sprocket does not require the use of any larger RDBMS such as MSSQL2005 in order to function, although MSSQL2005 is fully supported. Support for other database systems can be dropped in easily if desired, simply by implementing the data provider interface for each module to be supported.
  * Settings for Sprocket have been shifted into their own custom file Sprocket.config, which allows for a nice and readable, yet flexible configuration model. Modifying this file will invalidate the application, as with Web.Config.

# Beyond 0.5: The Road to 1.0 #

The road to 1.0 will require various extra developments, including (but not limited to) the following:

  * A base set of features such as blogging, forums and more.
  * Comprehensive file manager so that if desired, FTP requirements can be avoided once initial setup is complete. This would include a reasonably-functional text editor for editing templates and other files.
  * Installation should be a breeze. The system should detect that it has been installed, initiate database setup, log in as administrator and go to a clean front page placeholder, ready for setup.
  * All modules should include full support for SQLite and MSSQL2005
  * The admin system should be prettied up, including use of icons and helper graphics where possible.
  * A help system could exist in admin (is this necessary?)
  * The default website and sample module should be polished up.
  * SprocketCMS.com will be available by this time for people interested in using or developing with Sprocket.