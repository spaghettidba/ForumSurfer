**ForumSurfer**
===========
*A small RSS reader specifically designed to read and answer forum questions online.*

**ForumSurfer** lets you monitor your favourite forums and Q&A sites in a single place and get notified quickly of new questions that need attention.

Why building a RSS reader in 2016?
----------------------------------
[RSS is an old technology](https://en.wikipedia.org/wiki/RSS) and a simple [Google search for "free RSS reader"](https://www.google.com/search?q=free%20RSS%20reader) returns millions of results. So, why should you feel the need for a new RSS reader in 2016, when everything is said and done?
The answer is that most forums and Q&A sites use RSS, but no RSS reader (to date) was designed for this purpose.

### Straight to the source page ###
The new wave of RSS readers seems to be focusing more on displaying content in a magazine style rather than opening the source web pages. This is a great thing if you're interested in the content of the page, but it is really inconvenient if you need to interact in some way with the source page, which happens to be exactly what you do when you answer questions on online forums and Q&A sites.
In the best cases, you have to go through multiple clicks to open the source web page of an article: click the article to open it in the magazine-style viewer, right click to open a context menu, left click to select the "open source page" menu item. Too complicated.
**ForumSurfer** goes straight to the page source, no extra clicks needed.

### Updates quickly ###
Most RSS clients are online services, such as [Feedly](http://feedly.com/), or are backed by an online service and the client uses the API to display feeds in a smart/effective/glamorous way. [Nextgen Reader](http://nextmatters.com/apps/) is a brilliant example of such an application: it uses Feedly as data storage and pulls data using the Feedly API.
That's really great if you want to save bandwidth and have the online service taking care of updating all feeds for you. The only downside to that is that you lose control over the update interval: for instance, [Feedly updates feeds once every hour on average](https://www.feedly.com/fetcher.html). This can be good for fetching blog posts or articles, but it's not fast enough for forum questions. 
With forum questions, if you really want to help, it's a good thing to be early on the question, otherwise it will already be answered by somebody else by the time you read it. [*I don't mean that having questions answered by somebody else is a bad thing, it's just that working on unanswered questions is usually more helpful for the community*]. 
**ForumSurfer** can be configured to pull feeds up to every 1 minute, in order to be notified quickly of new questions.

### Boilerplate answers ###
Sometimes people post questions that are poorly worded or are missing crucial piece of information. What you do in those cases is writing a long-winded reply that explains what kind of information is needed, or simply points to a blog post that explains how to write a question properly. Writing that kind of reply over and over gets tiresome and it would be great if there was a convenient way to save a standard reply and pull it out when needed.
**ForumSurfer** offers the Boilerplate Answer feature: it lets you save your boilerplate answers and recall them with a single click. No more re-inventing the wheel.


----------

**Download**
--------
[Download ForumSurfer](https://github.com/spaghettidba/ForumSurfer/releases/latest) from the [releases page](https://github.com/spaghettidba/ForumSurfer/releases/latest).


----------

**Screenshots**
--------
**Main window**
The main window has a treeview with 3 levels: all feeds, host, feed. Clicking each level will display articles for that level.
Clicking an article on the central panel will open the article URL on the web browser on the right panel.
![Main window](https://raw.githubusercontent.com/spaghettidba/ForumSurfer/master/ForumSurfer/Images/ForumSurfer.png)

**Settings**
The settings flyout lets you configure global parameters and gives access to boilerplate answers. 
You can add (+ sign), delete (select and press DEL) or edit (doubleclick) boilerplate answers.
The settings panel also contains the "Import OPML" and "Export OPML" buttons.
![Main window](https://raw.githubusercontent.com/spaghettidba/ForumSurfer/master/ForumSurfer/Images/ForumSurfer_Settings.png)

**Boilerplate Answers Editor**
When you add or edit a boilerplate answer, the boilerplate answers editor appers.
![Main window](https://raw.githubusercontent.com/spaghettidba/ForumSurfer/master/ForumSurfer/Images/ForumSurfer_Settings_Boilerplate.png)

**Boilerplate Answers Usage**
To write the text of a boilerplate answer in the currently selected text control in your web browser, select a boilerplate answer by title from the dropdown button on the ribbon.
![Main window](https://raw.githubusercontent.com/spaghettidba/ForumSurfer/master/ForumSurfer/Images/ForumSurfer_Settings_Boilerplate_Use.png)

**Edit Feeds / Hosts**
Right click on any treeview item offers some options. Among others, "Edit" is interesting.
![Main window](https://raw.githubusercontent.com/spaghettidba/ForumSurfer/master/ForumSurfer/Images/ForumSurfer_Settings_Edit.png)

Edit Feeds lets you set the feed URI, Edit Host lets you set a zoom factor for that particular host. This is particularly useful in High-DPI displays, that usually don't render well all sites.
![Main window](https://raw.githubusercontent.com/spaghettidba/ForumSurfer/master/ForumSurfer/Images/ForumSurfer_Settings_SetZoom.png)


----------

**Getting started**
--------
[Download a sample OPML file](https://gist.github.com/spaghettidba/4b5a6d47bb61a16456c5400f4bc1cd7a) that includes all the major SQL Server forums and Q&A sites. Import the file from settings, Import OPML.

**Support**
--------
Email forumsurfer@sqlconsulting.it or tweet to [@spaghettidba](https://twitter.com/spaghettidba)
