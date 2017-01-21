# Galactic-Greed
-Realtime mobile multiplayer game and game server for graduation project-


### Summary
This project consists of two parts; developing the mobile game application and developing the server system which is connected to game application.
Aim of this project is providing users to continue their games from different devices. Users can create an account, log in and retrieve their last game status, position in the game etc. With this ability users can play their game from different devices without making a backup.
As a side goal, a real-time mobile multiplayer game is developed. A continuous data transfer is provided for the real-time game.


### Preview Codes and Making Changes
Project is developed using Unity 5.5.0f3 and the programmig language is C#. Mobile client app (core game) is developed with Unity, server program is written as a console program using C# and Visual Studio 2013. To provide communication, .Net Socket class is used for socket programming.


### Run
A server computer is needed. If the game is going to be played on LAN the ip address in the Assets/Scripts/ClientController.cs file must be updated with the ip of the server computer in the LAN. After the ip change, a build of the new Unity application is required. ServerApp.cs file must be compiled and run in the server computer.

### Some Screenshots From Game
<p>
  <img align="left" src="https://github.com/dogaminekaba/Galactic-Greed/blob/master/screenshots/ss1.png" width="300"/>
  <img align="center" src="https://github.com/dogaminekaba/Galactic-Greed/blob/master/screenshots/ss2.png" width="300"/>
</p>
<p>
  <img align="left" src="https://github.com/dogaminekaba/Galactic-Greed/blob/master/screenshots/ss4.png" width="300"/>
  <img align="center" src="https://github.com/dogaminekaba/Galactic-Greed/blob/master/screenshots/ss3.png" width="300"/>
</p>



Assets are from Kenney Assets and Unity Space Shooter.
<br/>
http://kenney.nl/assets
<br/>
https://www.assetstore.unity3d.com/en/#!/content/13866
