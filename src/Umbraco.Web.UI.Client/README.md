#Belle

Umbraco 7 UI, codename "Belle" Built on AngularJS, RequireJS and Twitter Bootstrap

##Introduction
Slides from the initial demonstration of Belle done at the Umbraco DK Fest can be found here: 

http://rawgithub.com/umbraco/Belle/master/Presentation/index.html
  

##Running the site with mocked data

This won't require any database or setup, as everything is running through node. All you have to do is install 
node and grunt on either windows or OSX and the entire setup is ready for you.


###Install node.js
We need node to run tests and automated less compiling and other automated tasks. go to http://nodejs.org. Node.js is a powerfull javascript engine, which allows us to run all our tests and tasks written in javascript locally.

*note:* On windows you might need to restart explorer.exe to register node.


###Install dependencies
Next we need to install all the required packages. This is done with the package tool, included with node.js, open /Umbraco.Belle.Client in cmd.exe or osx terminal and run the command:

	npm install

this will fetch all needed packages to your local machine.


###Install grunt globally
Grunt is a task runner for node.js, and we use it for all automated tasks in the build process. For convenience we need to install it globally on your machine, so it can be used directly in cmd.exe or the terminal.

So run the command:

	npm install grunt-cli -g

*note:* On windows you might need to restart explorer.exe to register the grunt cmd.

*note:* On OSX you might need to run:
	
	sudo npm install grunt-cli -g

Now that you have node and grunt installed, you can open `/Umbraco.Belle.Client` in either `cmd.exe` or terminal and run: 

	grunt dev

This will build the site, merge less files, run tests and create the /Build folder, and finally open the site in your
browser.


##Limitations
The current prototype simply uses in-memory storage, so no database dependencies. It is aimed at showing UI, not a complete functional client-server setup. 


##Project Structure

All project files are located in /umbraco.belle.client/src which only contains client-side files, everything 
related to asp.net are in umbraco.bell

after building Belle files are located in /build/belle, with all files following AngularJs 
conventions:

###Folders
- */belle/lib:* Dependencies
- */belle/js:* Application javascript files
- */belle/views/common/:* Main application views
- */belle/views/[sectioname]/pagename Editors html
- */belle/views/propertyeditors:* Property Editors html


###Files
- */belle/js/app.js:* Main umbraco application / modules
- */belle/js/loader.js:* yepnope configuration for dependencies
- */belle/js/routes.js:* Application routes
- */belle/js/umbraco.controllers.js:* Application controllers
- */belle/js/umbraco.services.js:* Application services
- */belle/js/umbraco.filters.js:* Application filters
- */belle/js/umbraco.directives.js:* Application directives
- */belle/js/umbraco.resources.js:* Application resources, like content, media, users, members etc
- */belle/js/umbraco.mocks.js:* Fake Application resources, for running the app without a server
 
##Getting started
The current app is built, following conventions from angularJs and bootstrap. To get started with the applicaton you will need to atleast know the basics of these frameworks 

###AngularJS
- Excellent introduction videos on http://www.egghead.io/
- Official guide at: http://docs.angularjs.org/guide/

###Require.js
- Introduction: http://javascriptplayground.com/blog/2012/07/requirejs-amd-tutorial-introduction
- Require.js website: http://requirejs.org/




