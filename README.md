# <div align="center"> <img src="./Data/Lightence_string_gradient.svg" width="350pt"/> </div>  

## Batchelor's Thesis
### Implementation of secure teleconference app with biometric authentication functions.  
  
IMPORTANT! This repository is an archive of a project for batchelor's thesis.  

## About application
Application contains:
 - server application
 - client application with GUI  

Application allows to create meetings with:
 - general text chat
 - text chat with specified person in the meeting
 - voice chat 
 - files sharing  

Authentication via login+password or login+biometric (face).  
Admin service allows to check statistics and simple management.  
Prepared two pricing options: basic (free) & premium. Premium needs valid key to upgrade user account.  



## Authors
 - [Piotr Kopycki](https://github.com/Aoxter)
    - connection between client's app and server
    - client's app logic
 - [Szymon Szczot](https://github.com/SzymonSzczot)
    - audio 
    - camera 
 - [Przemysław Wiktorowski](https://github.com/Przemko555)
    - client's app GUI (all XAML stuff)
 - [Marcin Złotek](https://github.com/zlociu)
    - server app
    - app logo
    - admin service


## Technology 
- [Azure](https://azure.microsoft.com)
- [ASP.NET Core 3.1](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-3.1)
- [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-3.1)
- [WPF](https://github.com/dotnet/wpf.git)
- [EmguCV](https://github.com/emgucv/emgucv.git)
- [NAudio](https://github.com/naudio/NAudio.git)

