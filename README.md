WebApp-WebAPI-OpenIDConnect-DotNet
==================================

This sample shows how to build an MVC web application that uses Azure AD for sign-in using the OpenID Connect protocol, and then calls a web API under the signed-in user's identity using tokens obtained via OAuth 2.0. This sample uses the OpenID Connect ASP.Net OWIN middleware and ADAL .Net.

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

## How To Run This Sample

To run this sample you will need:
- Visual Studio 2013
- An Internet connection
- An Azure subscription (a free trial is sufficient)

Every Azure subscription has an associated Azure Active Directory tenant.  If you don't already have an Azure subscription, you can get a free subscription by signing up at [http://wwww.windowsazure.com](http://www.windowsazure.com).  All of the Azure AD features used by this sample are available free of charge.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone git@github.com:AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet.git`

### Step 2:  Create a user account in your Azure Active Directory tenant

If you already have a user account in your Azure Active Directory tenant, you can skip to the next step.  This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.  If you create an account and want to use it to sign-in to the Azure portal, don't forget to add the user account as a co-administrator of your Azure subscription.

### Step 3:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample.  Each needs to be separately registered in your Azure AD tenant.

#### Register the TodoListService web API

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "TodoListService", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44321`.
9. For the App ID URI, enter `https://<your_tenant_name>/TodoListService`, replacing `<your_tenant_name>` with the name of your Azure AD tenant.  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
12. Using the Manage Manifest button in the drawer, download the manifest file for the application.
13. Add a permission to the application by replacing the appPermissions section with the block of JSON below.  You will need to create a new GUID and replace the example permissionId GUID.
14. Using the Manage Manfiest button, upload the updated manifest file.  Save the configuration of the app.

```JSON
"appPermissions": [
{
	"claimValue": "user_impersonation",
	"description": "Allow full access to the To Do List service on behalf of the signed-in user",
	"directAccessGrantTypes": [],
	"displayName": "Have full access to the To Do List service",
	"impersonationAccessGrantTypes": [
		{
			"impersonated": "User",
		    "impersonator": "Application"
		}
	],
	"isDisabled": false,
	"origin": "Application",
	"permissionId": "b69ee3c9-c40d-4f2a-ac80-961cd1534e40",
	"resourceScopeType": "Personal",
	"userConsentDescription": "Allow full access to the To Do service on your behalf",
	"userConsentDisplayName": "Have full access to the To Do service"
	}
],
```

#### Register the TodoListWebApp web app

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. In the drawer, click Add.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "TodoListWebApp", select "Web Application and/or Web API", and click next.
8. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44322/`.  NOTE:  It is important, due to the way Azure AD matches URLs, to ensure there is a trailing slash on the end of this URL.  If you don't include the trailing slash, you will receive an error when the application attempts to redeem an authorization code.
9. For the App ID URI, enter `https://<your_tenant_name>/TodoListWebApp`, replacing `<your_tenant_name>` with the name of your Azure AD tenant.  Click OK to complete the registration.
10. While still in the Azure portal, click the Configure tab of your application.
11. Find the Client ID value and copy it aside, you will need this later when configuring your application.
12. Create a new key for the application.  Save the configuration so you can view the key value.  Save this aside for when you configure the project in Visual Studio.
13. In the Permissions to Other Applications configuration section, select the TodoListService application, and add the full access delegated permission.  Save the configuration.

### Step 4:  Configure the sample to use your Azure AD tenant

#### Configure the TodoListService project

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:Audience` and replace the value with the App ID URI you registered earlier, for example `https://<your_tenant_name>/TodoListService`.
5. Find the app key `ida:ClientId` and replace the value with the Client ID for the TodoListService from the Azure portal.

#### Configure the TodoListWebApp project

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:ClientId` and replace the value with the Client ID for the TodoListWebApp from the Azure portal.
5. Find the app key `ida:AppKey` and replace the value with the key for the TodoListWebApp from the Azure portal.
6. If you changed the base URL of the TodoListWebApp sample, find the app key `ida:PostLogoutRedirectUri` and replace the value with the new base URL of the sample.
7. Find the app key `todo:TodoListBaseAdress` ane make sure it has the correct value for the address of the TodoListService project.
8. Find the app key `todo:TodoListResourceId` and replace the value with the App ID URI registered for the TodoListService, for example `https://<your_tenant_name>/TodoListService`.

### Step 5:  Trust the IIS Express SSL certificate

Since the web API is SSL protected, the client of the API (the web app) will refuse the SSL connection to the web API unless it trusts the API's SSL certificate.  Use the following steps in Windows Powershell to trust the IIS Express SSL certificate.  You only need to do this once.  If you fail to do this step, calls to the TodoListService will always throw an unhandled exception where the inner exception message is:

"The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."

To configure your computer to trust the IIS Express SSL certificate, begin by opening a Windows Powershell command window as Administrator.

Query your personal certificate store to find the thumbprint of the certificate for `CN=localhost`:

```
PS C:\windows\system32> dir Cert:\LocalMachine\My


    Directory: Microsoft.PowerShell.Security\Certificate::LocalMachine\My


Thumbprint                                Subject
----------                                -------
C24798908DA71693C1053F42A462327543B38042  CN=localhost
```

Next, add the certificate to the Trusted Root store:

```
PS C:\windows\system32> $cert = (get-item cert:\LocalMachine\My\C24798908DA71693C1053F42A462327543B38042)
PS C:\windows\system32> $store = (get-item cert:\Localmachine\Root)
PS C:\windows\system32> $store.Open("ReadWrite")
PS C:\windows\system32> $store.Add($cert)
PS C:\windows\system32> $store.Close()
```

You can verify the certificate is in the Trusted Root store by running this command:

`PS C:\windows\system32> dir Cert:\LocalMachine\Root`

### Step 6:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

Explore the sample by signing in, clicking the User Profile and To Do List links, adding items to the To Do list, signing out, and starting again.

## How To Deploy This Sample to Azure

To deploy the TodoListService and TodoListWebApp to Azure Web Sites, you will create two web sites, publish each project to a web site, and update the TodoListWebApp to call the web site instead of IIS Express.

### Create and Publish the TodoListService to an Azure Web Site

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Web Sites in the left hand nav.
3. Click New in the bottom left hand corner, select Compute --> Web Site --> Quick Create, select the hosting plan and region, and give your web site a name, e.g. todolistservice-contoso.azurewebsites.net.  Click Create Web Site.
4. Once the web site is created, click on it to manage it.  For this set of steps, download the publish profile and save it.  Other deployment mechanisms, such as from source control, can also be used.
5. Switch to Visual Studio and go to the TodoListService project.  Right click on the project in the Solution Explorer and select Publish.  Click Import, and import the publish profile that you just downloaded.
6. On the Connection tab, update the Destination URL so that it is https, for example https://todolistservice-skwantoso.azurewebsites.net.  Click Next.
7. On the Settings tab, make sure Enable Organizational Authentication is NOT selected.  Click Publish.
8. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Create a TodoListWebApp Azure Web Site

1. Navigate to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Web Sites in the left hand nav.
3. Click New in the bottom left hand corner, select Compute --> Web Site --> Quick Create, select the hosting plan and region, and give your web site a name, e.g. todolistwebapp-contoso.azurewebsites.net.  Click Create Web Site.
4. Once the web site is created, click on it to manage it.  For this set of steps, download the publish profile and save it.  Other deployment mechanisms, such as from source control, can also be used.

### Update the TodoListWebApp to call the TodoListService Running in Azure Web Sites

1. In Visual Studio, go to the TodoListWebApp project.
2. Open `web.config`.  Two changes are needed - first, update the `todo:TodoListBaseAddress` key value to be the address of the website you published, e.g. https://todolistservice-skwantoso.azurewebsites.net.  Second, update the `ida:PostLogoutRedirectUri` key value to be the address of website you published, e.g. https://todolistwebapp-skwantoso.azurewebsites.net.

### Update the Application Configurations in the Directory Tenant

1. Navigate to the [Azure management portal](https://manage.windowsazure.com).
2. In the left hand nav, clink on Active Directory and select your tenant.
3. On the applications tab, select the TodoListService application.
4. On the Configure tab, update the Sign-On URL and Reply URL fields to the address of your service, for example https://todolistservice-skwantoso.azurewebsites.net.  Save the configuration.
5. Navigate to the TodoListWebApp application within your Active Directory tenant.
6. On the Configure tab, update the Sign-On URL and the Reply URL fields to the address of your web app, for example https://todolistwebapp-skwantoso.azurewebsites.net.  Save the configuration.

### Publish the TodoListWebApp to the Azure Web Site

1. In Visual Studio, right click on the TodoListWebApp project in the Solution Explorer and select Publish.  Click Import, and import the publish profile that you downloaded.
2. On the Connection tab, update the Destination URL so that it is https, for example https://todolistwebapp-skwantoso.azurewebsites.net.  Click Next.
3. On the Settings tab, make sure Enable Organizational Authentication is NOT selected.  Click Publish.
4. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

NOTE:  Remember, the To Do list is stored in memory in this TodoListService sample.  Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied.  Also, if you increase the instance count of the web site, requests will be distributed among the instances and the To Do will not be the same on each instance.

## About The Code

Coming soon.

## How To Recreate This Sample

First, in Visual Studio 2013 create an empty solution to host the  projects.  Then, follow these steps to create each project.

### Creating the TodoListService Project

1. In the solution, create a new ASP.Net MVC web API project called TodoListService and while creating the project, click the Change Authentication button, select Organizational Accounts, Cloud - Single Organization, enter the name of your Azure AD tenant, and set the Access Level to Single Sign On.  You will be prompted to sign-in to your Azure AD tenant.  NOTE:  You must sign-in with a user that is in the tenant; you cannot, during this step, sign-in with a Microsoft account.
2. In the `Models` folder add a new class called `TodoItem.cs`.  Copy the implementation of TodoItem from this sample into the class.
3. Add a new, empty, Web API 2 controller called `TodoListController`.
4. Copy the implementation of the TodoListController from this sample into the controller.  Don't forget to add the `[Authorize]` attribute to the class.
5. In `TodoListController` resolving missing references by adding `using` statements for `System.Collections.Concurrent`, `TodoListService.Models`, `System.Security.Claims`.

### Creating the TodoListWebApp Project

1. In the solution, create a new ASP.Net MVC web application called TodoListWebApp with Authentication set to No Authentication.
2. Set SSL Enabled to be True.  Note the SSL URL.
3. In the project properties, Web properties, set the Project Url to be the SSL URL.
4. Add the following ASP.Net OWIN middleware NuGets: Microsoft.IdentityModel.Protocol.Extensions, System.IdentityModel.Tokens.Jwt, Microsoft.Owin.Security.OpenIdConnect, Microsoft.Owin.Security.Cookies, Microsoft.Owin.Host.SystemWeb.
5. Add the Active Directory Authentication Library NuGet (`Microsoft.IdentityModel.Clients.ActiveDirectory`).
6. In the `App_Start` folder, create a class `Startup.Auth.cs`.  You will need to remove `.App_Start` from the namespace name.  Replace the code for the `Startup` class with the code from the same file of the sample app.  Be sure to take the whole class definition!  The definition changes from `public class Startup` to `public partial class Startup`.
7. Right-click on the project, select Add,  select "OWIN Startup class", and name the class "Startup".  If "OWIN Startup Class" doesn't appear in the menu, instead select "Class", and in the search box enter "OWIN".  "OWIN Startup class" will appear as a selection; select it, and name the class `Startup.cs`.
8. In `Startup.cs`, replace the code for the `Startup` class with the code from the same file of the sample app.  Again, note the definition changes from `public class Startup` to `public partial class Startup`.
9. In the `Views` --> `Shared` folder, create a new partial view `_LoginPartial.cshtml`.  Replace the contents of the file with the contents of the file of same name from the sample.
10. In the `Views` --> `Shared` folder, replace the contents of `_Layout.cshtml` with the contents of the file of same name from the sample.  Effectively, all this will do is add a single line, `@Html.Partial("_LoginPartial")`, that lights up the previously added `_LoginPartial` view.
11. Create a new empty controller called `AccountController`.  Replace the implementation with the contents of the file of same name from the sample.
12. If you want the user to be required to sign-in before they can see any page of the app, then in the `HomeController`, decorate the `HomeController` class with the `[Authorize]` attribute.  If you leave this out, the user will be able to see the home page of the app without having to sign-in first, and can click the sign-in link on that page to get signed in.
13. In the `Models` folder add a new class called `TodoItem.cs`.  Copy the implementation of TodoItem from this sample into the class.
14. In the `Models` folder add a new class called `UserProfile.cs`.  Copy the implementation of UserProfile from this sample into the class.
15. In the project, create a new folder called `Utils`.  In the folder, create a new class called `NaiveSessionCache.cs`.  Copy the implementation of the class from the sample.
16. Add a new empty MVC5 controller TodoListController to the project.  Copy the implementation of the controller from the sample.  Remember to include the [Authorize] attribute on the class definition.
17. Add a new empty MVC5 controller UserProfileController to the project.  Copy the implementation of the controller from the sample.  Again, remember to include the [Authorize] attribute on the class definition.
18. In `Views` --> `TodoList` create a new view, `Index.cshtml`, and copy the implementation from this sample.
19. In `Views` --> `UserProfile` create a new view, `Index.cshtml`, and copy the implementation from this sample.
20. In the shared `_Layout` view, make sure the Action Links for Profile and To Do List that are in the sample have been added.
21. In `web.config`, in `<appSettings>`, create keys for `ida:ClientId`, `ida:AppKey`, `ida:AADInstance`, `ida:Tenant`, `ida:PostLogoutRedirectUri`, `ida:GraphResourceId`, and `ida:GraphUserUrl` and set the values accordingly.  For the public Azure AD, the value of `ida:AADInstance` is `https://login.windows.net/{0}` the value of `ida:GraphResourceId` is `https://graph.windows.net`, and the value of `ida:GraphUserUrl` is `https://graph.windows.net/{0}/me?api-version=2013-11-08`.
22. In `web.config` in `<appSettings>`, create keys for `todo:TodoListResourceId` and `todo:TodoListBaseAddress` and set the values accordinly.
23. In `web.config` add this line in the `<system.web>` section: `<sessionState timeout="525600" />`.  This increases the ASP.Net session state timeout to it's maximum value so that access tokens and refresh tokens cache in session state aren't cleared after the default timeout of 20 minutes.

Finally, in the properties of the solution itself, set both projects as startup projects.
