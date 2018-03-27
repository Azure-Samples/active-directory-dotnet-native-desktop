---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: .NET Framework 4.5 Console 
service: ASP.NET Web API
endpoint: AAD V1
---
# Integrating Azure AD into a Windows desktop application using interactive authentication

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/49/badge)

## About this sample

### Scenario

This sample demonstrates a .Net WPF application calling a web API that is secured using Azure AD.

1. The .Net TodoListClient WPF application uses the Active Directory Authentication Library (ADAL) to obtain a JWT access token from Azure Active Directory (Azure AD) through the OAuth 2.0 protocol:
2. The access token is used as a bearer token to authenticate the user when calling the /todolist endpoint of the TodoListService web API.

![Topology](./ReadmeFiles/topology.png)

Once you've started the TodolistService, you can run the TodoListClient WPF application, click on the *sign-in* button, and then enter items in a todo list. When you close the application and reopen it, the list is still displayed, as long as the service is running (it maintains the todo list in memory), or until you click on the *Clear Cache* button. At this point, you'll need to sign in again (and can use another identity). You will manipulate the todo list of the identity you signed-in with.

![TodoListClient](./ReadmeFiles/TodolistClient.png)

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

> Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## How To Run This Sample

To run this sample, you'll need:

- [Visual Studio 2017](https://aka.ms/vsdownload)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account. Therefore, if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-native-client.git`

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- either follow the steps in the paragraphs below (Step 2 and Step 3)
- or use PowerShell scripts that **automatically** create for you the Azure AD applications and related objects (passwords, permissions, dependencies) and modify the projects' configuration files. If you want to do use this automation, read the instructions in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

#### Register the TodoListService web API

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **More Services** in the left-hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **Add**.
5. Enter a friendly name for the application, for example 'TodoListService' and select 'Web Application and/or Web API' as the Application Type. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44321`. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on **Settings**, and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.
8. For the App ID URI, enter https://\<your_tenant_name\>/TodoListService, replacing \<your_tenant_name\> with the name of your Azure AD tenant.

#### Register the TodoListClient app

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **More Services** in the left-hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **Add**.
5. Enter a friendly name for the application, for example 'TodoListClient-DotNet' and select 'Native' as the Application Type. For the redirect URI, enter `https://TodoListClient`. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on Settings, and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.
8. Configure Permissions for your application - in the Settings menu, choose the 'Required permissions' section, click on **Add**, then **Select an API**, and type 'TodoListService' in the textbox. Then, click on  **Select Permissions** and select 'Access TodoListService'.

### Step 3:  Configure the sample to use your Azure AD tenant

#### Configure the TodoListService project

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:Audience` and replace the value with the App ID URI you registered earlier, for example `https://<your_tenant_name>/TodoListService`.

#### Configure the TodoListClient project

1. Open `app.config`.
2. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
3. Find the app key `ida:ClientId` and replace the value with the Client ID for the TodoListClient from the Azure portal.
4. Find the app key `ida:RedirectUri` and replace the value with the Redirect URI for the TodoListClient from the Azure portal, for example `http://TodoListClient`.
5. Find the app key `todo:TodoListResourceId` and replace the value with the App ID URI of the TodoListService, for example `https://<your_tenant_name>/TodoListService`
6. Find the app key `todo:TodoListBaseAddress` and replace the value with the base address of the TodoListService project.

### Step 4:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

Explore the sample by signing in, adding items to the To Do list, removing the user account (by clicking on "Clear Cache"), and starting again.  Notice that if you stop the application without removing the user account, the next time you run the application you won't be prompted to sign in again. The sample implements a persistent cache for ADAL, and remembers the tokens from the previous run.

## How To Deploy This Sample to Azure

To deploy the TodoListService to Azure Web Sites, you will need to:

- create an Azure Web Site
- publish the TodoListService to the web site, and
- update the TodoListClient to call the web site instead of IIS Express.

### Create and Publish the TodoListService to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
2. Click New in the top left-hand corner, select Web + Mobile --> Web App, select the hosting plan and region, and give your web site a name, for example, todolistservice-contoso.azurewebsites.net.  Click Create Web Site.
3. Once the web site is created, click on it to manage it.  For this set of steps, download the publish profile and save it.  Other deployment mechanisms, such as from source control, can also be used.
4. Switch to Visual Studio and go to the TodoListService project.  Right click on the project in the Solution Explorer and select Publish.  Click Import, and import the publish profile that you just downloaded.
5. On the Connection tab, update the Destination URL so that it is https, for example https://todolistservice-skwantoso.azurewebsites.net.  Click Next.
6. On the Settings tab, make sure Enable Organizational Authentication is NOT selected.  Click Publish.
7. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory Tenant Application Registration

1. Navigate to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant containing the TodoListService application.
3. On the applications tab, select the TodoListService application.
4. From the Settings -> Properties and Settings -> Reply URLs menus, update the Sign-On URL, and Reply URL fields to the address of your service, for example https://todolistservice-skwantoso.azurewebsites.net.  Save the configuration.

### Update the TodoListClient to call the TodoListService Running in Azure Web Sites

1. In Visual Studio, go to the TodoListClient project.
2. Open `app.config`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published, for example, https://todolistservice-skwantoso.azurewebsites.net.
3. Run the client!  If you are trying multiple different client types (for example, .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

NOTE: Remember, the To Do list is stored in memory in this TodoListService sample. Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied. Also, if you increase the instance count of the web site, requests will be distributed among the instances and the To Do will not be the same on each instance.

## About The Code

### On the client slide

The code using ADAL.NET is in the [TodoListClient/MainWindow.xaml.cs](TodoListClient/MainWindow.xaml.cs) file in the `SignIn` method. See [More information](#more-information) below
for details on how this work.

Also, to avoid you re sign in each time your run the client application, the code provides a Token cache serialization located in [TodoListClient/FileCache.cs](TodoListClient/FileCache.cs)

### On the service side

The `Startup.cs` file contains a partial implementation of the `Startup` class, which Configration() method calls ConfigureAuth()

```C#
public partial class Startup
{
    public void Configuration(IAppBuilder app)
    {
        ConfigureAuth(app);
    }
}
```

In the `App_Start\Startup.Auth.cs` file the `ConfigureAuth(…)` method declare the uses of Windows Azure Active Directory bearer authentication and sets the application coordinates to communicate with Azure AD as  `WindowsAzureActiveDirectoryBearerAuthenticationOptions`.

```C#
public void ConfigureAuth(IAppBuilder app)
{
    app.UseWindowsAzureActiveDirectoryBearerAuthentication(
        new WindowsAzureActiveDirectoryBearerAuthenticationOptions
        {
            Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
            }
        });
}
```

- The `[Authorize]` attributes on the controller  protect the controllers and actions with JWT bearer authentication.  See for instance the `Controllers\TodoListController.cs` class with an authorize attribute.  This will force the user to sign in before accessing that page.

```C#
[Authorize]
public class TodoListController : ApiController
{
```

- When an authorized caller successfully invokes one of the `TodoListController` APIs, the action might need access to information about the caller.  OWIN provides access to the claims inside the bearer token via the `ClaimsPrincpal` object.
- A common requirement for web APIs is to validate the "scopes" present in the token - this ensures that the end user has consented to the permissions required to access the Todo List Service:

```C#
public IEnumerable<TodoItem> Get()
{
    // user_impersonation is the default permission exposed by applications in AAD
    if (ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value != "user_impersonation")
    {
        throw new HttpResponseException(new HttpResponseMessage {
          StatusCode = HttpStatusCode.Unauthorized,
          ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found"
        });
    }
    ...
}
```

## How To Recreate This Sample

First, in Visual Studio 2017 create an empty solution to host the  projects.  Then, follow these steps to create each project.

### Creating the TodoListService Project

1. In the solution, create a new ASP.Net MVC web API project called `TodoListService` and while creating the project, click the Change **Authentication** button, select **Organizational Accounts, Cloud - Single Organization**, enter the name of your Azure AD tenant, and set the Access Level to **Single Sign On**.  You will be prompted to sign in to your Azure AD tenant.  NOTE:  You must sign in with a user that is in the tenant; you cannot, during this step, sign in with a Microsoft account.
2. In the  folder, add a new class called `TodoItem.cs`.  Copy the implementation of TodoItem from this sample into the class.
3. Add a new, empty, Web API 2 controller called `TodoListController`.
4. Copy the implementation of the TodoListController from this sample into the controller.  Don't forget to add the `[Authorize]` attribute to the class.
5. In `TodoListController` resolving missing references by adding `using` statements for `System.Collections.Concurrent`, `TodoListService.Models`, `System.Security.Claims`.

### Creating the TodoListClient Project

1. In the solution, create a new Windows --> WPF Application called TodoListClient.
2. Add the (stable release) Active Directory Authentication Library (ADAL) NuGet, `Microsoft.IdentityModel.Clients.ActiveDirectory` to the project.
3. Add  assembly references to `System.Net.Http`, `System.Web.Extensions`, `System.Security`, and `System.Configuration`.
4. Add a new class to the project called `TodoItem.cs`.  Copy the code from the sample project file of the same name into this class, completely replacing the code in the file in the new project.
5. Add a new class to the project called `FileCache.cs`.  Copy the code from the sample project file of the same name into this class, completely replacing the code in the file in the new project.
6. Copy the markup from `MainWindow.xaml' in the sample project into the file of the same name in the new project, completely replacing the markup in the file in the new project.
7. Copy the code from `MainWindow.xaml.cs` in the sample project into the file of the same name in the new project, completely replacing the code in the file in the new project.
8. In `app.config` create keys for `ida:AADInstance`, `ida:Tenant`, `ida:ClientId`, `ida:RedirectUri`, `todo:TodoListResourceId`, and `todo:TodoListBaseAddress` and set them accordingly.  For the global Azure cloud, the value of `ida:AADInstance` is `https://login.microsoftonline.com/{0}`.

Finally, in the properties of the solution itself, set both projects as startup projects.

## More information

For more information, see ADAL.NET's conceptual documentation:

- [Recommended pattern to acquire a token](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token#recommended-pattern-to-acquire-a-token)
- [Acquiring tokens interactively in public client applications](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Acquiring-tokens-interactively---Public-client-application-flows)
- [Customizing Token cache serialization](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Token-cache-serialization)
