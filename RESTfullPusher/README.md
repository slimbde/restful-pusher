# RESTfullPusher ~ Grigoriy Dolgiy 2022

This is a grand revision of `Rozhkov Yasha` WCF service. No garbage, nothing redundant.
The service is a WCF service hosted at IIS. Why WCF? Because it is written for Windows Server 2003. The maximum available .Net Framework is 4.
I would rather implement an MVC controller, but it requires .Net Framework 4.5.. So WCF would do with poor .Net Framework 4.


## Architecture
- Follow factory mechanizm and manipulate interfaces. Leave implementations to the factory.
- Put solution models to the `AppModels` project


## Logging
- Exceptions are written to the EventLog. But there might be a terrible bug when you try to run it for the first time at the server.
- The service itself might not be allowed to create new Event source and event log. So precede deploying this app with creating apt event log.


## Reason
- In the very moment the old, ugly mechanizm dispatching screenshots to the Dispatcher server is running. The mechanizm grabs screenshots from  
  the L2@CCM4 `tmp` folder using shared folder approach. But sometimes shit happens and it stucks being blocked by system.
- To cover my (and your) ass I decided to get rid of this approach and use HTTP to dispatch these screenshots.
- Now. Every single ARM is supplied with `ScreenCap` program running on it and sending screenshots via HTTP to the PusherService at L2 server.
- Pusher service (this service) is bound to the `176.16.71.190` IP, so no one able to reach this server over `10.2...` interface.
- So screenshots are received from `176.16...` interface and posted to the `10.2...` interface to the Dispatcher server (10.2.59.150)


## Deploying
- Right click on the main project (RESTfullPusher) and pick Publish
- In the publish section select appropriate profile (if you can't use ftp, just choose a folder profile)
- Move the release to the destination server and bound it in the IIS
- Done.


## Config
- Recipients are listed in Web.config file in Destinations section
- The app implemented so it allows to apply web config changes on each Handle action. It means whenever you changed config values it picks them up immediately.

