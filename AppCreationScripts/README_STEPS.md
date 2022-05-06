---
page_type: sample
name: How to use an authenticated user's credentials for log-in to an Azure SQL Database from Blazor Web Server App
services: active-directory
platforms: dotnet
urlFragment: ms-identity-dotnet-blazor-azure-sql
description: This sample demonstrates how to use access token obtained from AAD for connecting to Azure SQL Server database as a user that is logged in into the application.
languages:
 - csharp
 - t-sql
products:
 - aspnet-core
 - blazor
 - azure-active-directory
 - azure-sql
---

# How to use an authenticated user's credentials for log-in to an Azure SQL Database from Blazor Web Server App

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

Table Of Contents

* [Scenario](#Scenario)
* [Prerequisites](#Prerequisites)
* [Setup the sample](#Setup-the-sample)
* [Troubleshooting](#Troubleshooting)
* [Using the sample](#Using-the-sample)
* [About the code](#About-the-code)
* [How the code was created](#How-the-code-was-created)
* [How to deploy this sample to Azure](#How-to-deploy-this-sample-to-Azure)
* [Next Steps](#Next-Steps)
* [Contributing](#Contributing)
* [Learn More](#Learn-More)

## Scenario

![Scenario Image](ReadmeFiles/topology.png)



## Prerequisites

- Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- Azure subscription and Tenant with at least one user created in it
- Azure [SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart)




## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/ms-identity-dotnet-blazor-azure-sql.git
```

or download and extract the repository .zip file.

>:warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder
You don't have to change current folder. 


### Step 3: Application Registration

There is one project in this sample. To register it, you can:

Follow the [manual steps](#Manual-steps)

**OR**

#### Run automation scripts

* use PowerShell scripts that:
  * **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  * modify the projects' configuration files.

  <details>
   <summary>Expand this section if you want to use this automation:</summary>

    > **WARNING**: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
    1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
    1. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
    1. For interactive process - in PowerShell run:

       ```PowerShell
       cd .\AppCreationScripts\
       .\Configure.ps1 -TenantId "[Optional] - your tenant id" -Environment "[Optional] - Azure environment, defaults to 'Global'"
       ```

    1. In case the previous script fails with error about duplicate App Registration, you might want to run the next cleanup script prior to re-running Configure.ps1

       ```powershell
       cd .\AppCreationScripts\
       .\Cleanup.ps1
       ```

       > Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md)
       > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

  </details>

#### Manual Steps

 > Note: skip this part if you've just used Automation steps

Follow the steps below for manually register and configure your apps

<details>
   <summary>Expand this section if you want to use this automation:</summary>

   1. Sign in to the [Azure portal](https://portal.azure.com).
   1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

##### Register the client app (ClientApp-blazor-azuresql)

   1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD** service.
   1. Select the **App Registrations** blade on the left, then select **New registration**.
   1. In the **Register an application page** that appears, enter your application's registration information:
      * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `ClientApp-blazor-azuresql`.
   1. Under **Supported account types**, select **Accounts in this organizational directory only**
   1. Click **Register** to create the application.
   1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
   1. In the app's registration screen, select **Authentication** in the menu.
      * If you don't have a platform added, select **Add a platform** and select the **Web** option.
   1. In the **Redirect URI** section enter the following redirect URIs: 
      * `https://localhost:44348/`
      * `https://localhost:44348/signin-oidc`
   1. In the **Front-channel logout URL** section, set it to `https://localhost:44348/signout-oidc`.
   1. Select **ID tokens (used for implicit and hybrid flows)** checkbox.
   1. Click **Save** to save your changes.
   1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where you can generate secrets and upload certificates.
   1. In the **Client secrets** section, select **New client secret**:
      * Optionally you can type a key description (for instance `app secret`),
      * Select recommended Expire duration.
      * The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
      * You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.

##### Configure the client app (ClientApp-blazor-azuresql) to use your app registration

   Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

   > In the steps below, "ClientID" is the same as "Application ID" or "AppId".

   1. Open the `Client\appsettings.json` file.
      1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
      2. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
      3. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `ClientApp-blazor-azuresql` app copied from the Azure portal.
      4. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `ClientApp-blazor-azuresql` copied from the Azure portal.

  **For more information, visit** [Register Application AAD](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)

  </details>

### Step 4: Running the sample

 To run the sample, run the next commands:

```console
    You don't have to change current folder. 
    dotnet run
```

## Troubleshooting

<details>
 <summary>Expand for troubleshooting info</summary>

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `adal` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).
</details>

## Using the sample

<details>
 <summary>Expand to see how to use the sample</summary>
 
 
Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

[Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)
</details>
## About the code

<details>
 <summary>Expand the section</summary>


</details>

## How the code was created

<details>
 <summary>Expand the section</summary>

### Creating the client web app (TodoListClient)



</details>

## How to deploy this sample to Azure

<details>
 <summary>Expand the section</summary>



</details>


## Next Steps

Learn how to:

* [Change your app to sign-in users from any organization or any Microsoft accounts](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-3-AnyOrgOrPersonal)
* [Enable users from National clouds to sign-in to your application](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-4-Sovereign)
* [Enable your Web App to call a Web API on behalf of the signed-in user](https://github.com/Azure-Samples/ms-identity-dotnetcore-ca-auth-context-app)

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

* Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* Building Zero Trust ready apps](https://aka.ms/ztdevsession)

For more information, visit the following links:

 *To lean more about the application registration, visit:
  *[Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  *[Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  *[Quickstart: Configure an application to expose web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

  *To learn more about the code, visit:
  *[Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  *[Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  *[Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

  *To learn more about security in aspnetcore,
  *[Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
  *[AuthenticationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder)
  *[Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory)



