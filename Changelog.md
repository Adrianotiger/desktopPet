# Changelog
 
 <h3>Version 1.2.3<sup>04 oct 2019</sup>:</h3> 
 
 - Fixed cutting problem
 - Auto update over GitHub for portable version
 - Compiled with Visual Studio 2019 and with new libraries
 - UWP: better option dialog 
 
 <h3>Version 1.2.2<sup>08 jul 2019</sup>:</h3> 
 
* Improved border detection, also top and bottom border (not only left and right) 
* Some times on respawn, pet was cut in a wrong way 
 
 <h3>Version 1.2.1<sup>02 jul 2019</sup>:</h3>  
 
* Improved border detection, now the pet will walk until the border was reached, not before.
* Improved multiscreen, pet will not be visible on the second screen when it leaving the area to respaw
* Improved spawn, it should not blink anymore before spawning again
* NEKO, another mate from 1995 is now available
* GITHUB-Mates! From now, UWP, portable and javascript applications can access the GitHub repository to download new pets.
* Please use the offline editor before uploading the new animation on GitHub as it will validate your xml and check it. 
 
 
 <h3>Version 1.2.0<sup>12 jun 2019</sup>:</h3>  
 
- Same version as the UWP (Windows 10) version - now as portable (see GitHub).
- Without certificate, maybe the antivirus will give a warning
- Multiscreen added (set it in the option dialog)
- If the application is not installed, the settings are saved in the application folder
- Support for new animations format (https instead of http), since in future the animations will be moved to github.
- NEW OFFLINE EDITOR! CHECK THE GITHUB-PROJECT TO DOWNLOAD THE PREVIEW. 
 

 <h3>Version 1.0.7<sup>13 jan 2018</sup>:</h3>  
 
- Improvement in the option dialog (help over GitHub).
- My certificate will expire in 5 days and the price is too high now... Don't know if I will ever pay a new certificate :P
 
 
 <h3>Version 1.0.6<sup>12 aug 2017</sup>:</h3>  
 
- Under options you can start multiple pets when the application starts.
 
  
 <h3>Version 1.0.5<sup>07 mar 2017</sup>:</h3>
 
- Multiple childs can be created from the same animation ID
- Online editor can check your xml values, so it gives less errors on the application once released
- Check the new animations under Options!  
 
 
 <h3>Version 1.0.4<sup>22 feb 2017</sup>:</h3>
 
- Solved a bug with the icon, if you downloaded an animation with a wrong icon it was not possible to run the app again
 
 
 <h3>Version 1.0.3<sup>08 feb 2017</sup>:</h3>
 
- Developers can generate agraphically image from the XML animation: see blog (thank you Robin)
- Improved memory usage (thank you Robin)
- Offset of the pet was not calculated correctly
 
 
 <h3>Version 1.0.2<sup>02 feb 2017</sup>:</h3>
 
- Solved a bug when the sheep was falling (app crashed)
- Window will not be set on foreground anymore (you can set it back under options, if you want it)
- The entire documentation can be found online on github now.
 
 
 <h3>Version 1.0.1<sup>21 jan 2017</sup>:</h3>
 
- Solved a bug with the fullscreen (pet was set in background even if there wasn't a fullscreen window)
- Increased stability with wrong formatted animations
- New certificate to sign open source application (last one was expired)
 
 
 <h3>Version 1.0.0<sup>01 jan 2017</sup>:</h3>
 
- Version 1.0! This application works without any big bugs now :D
- Fullscreen detection, if a movie is playing, the sheep will not be on top most anymore
- Bring to top, if you click on the tray icon, the sheep will be on top most again
 
 
 <h3>Version 0.9.8 (beta)<sup>13 aug 2016</sup>:</h3>
 
- If sound player fails, you can see the error on options.
- Corrected the position of the pet if you have the taskbar on the top.
- Removed the async sound, please use the gSheep pet (under option), it is much better than my sheep :P
 
 
 <h3>Version 0.9.7 (beta)<sup>19 jun 2016</sup>:</h3>
 
- Default animation had too much sounds
- Pet can create childs and child can create sub-childs.
 
 
 <h3>Version 0.9.6 (beta)<sup>14 jun 2016</sup>:</h3>
 
- Border collision sets the pet on the right position.
- Possibility to add sounds (NAudio was implemented)
- Option dialog redesigned
 
   
 <h3>Version 0.9.5 (beta)<sup>17 apr 2016</sup>:</h3>
 
- Child can't be picked anymore
- Animation will not steal anymore current window focus when created
- Application update notification appear only if the application is installed
- The last frame of a single animation was not executed correctly
- Solved bug with offsetY in the XML animation
 
 
 <h3>Version 0.9.4 (beta)<sup>16 apr 2016</sup>:</h3>
 
- Solved a bug with screen border detection
- If an update is available, the changelog will be show
 

 <h3>Version 0.9.3 (beta)<sup>14 apr 2016</sup>:</h3>
 
- Double click to close a single pet
- Publish and create an installer with your PET without see any sheep on the app!
- Solved child animations bugs
- A respawn will be executed when pet is outside the screen and not on collision with the borders
- <em>3 new animations for the sheep</em>
- General improvements, like fading pet or installing application
 

 <h3>Version 0.9.2 (beta)<sup>25 mar 2016</sup>:</h3>
 
- Optimized source code
- Reading XML with XML serialisation instead of the internal parser
- Smoother movements of the pet 
 

 <h3>Version 0.9.1 (beta)<sup>25 feb 2016</sup>:</h3>
 
- Solved a bug with the Culture (countries with "," as comma)
 
 
 <h3>Version 0.9.0 (beta)<sup>04 feb 2016</sup>:</h3>
 
- Solved falling bug (window was always take to front)
- Installer integrated in the application
- Signed with SHA-2 Certum Open Source Certificate
 
 
 <h3>Version 0.8.8 (beta)<sup>03 feb 2016</sup>:</h3>
 
- Code comments updated: <a href='https://github.com/Adrianotiger/desktopPet/blob/master/Docs/Documentation.chm?raw=true'>API documentation</a>
- If a pet can't be spawn, a default position will be used. 
- About box shows you the information of the current animation.
- Possibility to <b>install</b> application directly from the app.
- Application can be copied in the windows autostart menu!
 
 
 <h3>Version 0.8.5 (beta)<sup>27 jan 2016</sup>:</h3>
 
- Code is commented and uploaded here: <a href='https://github.com/Adrianotiger/desktopPet/blob/master/Docs/Documentation.chm?raw=true'>API documentation</a>
- Window will not flash anymore when the sheep fall over it
- Sheep XML animation was optimized
- Solved a little bug when an animation was over
- Application can be executed twice
 
 
 <h3>Version 0.8.1 (beta)<sup>21 jan 2016</sup>:</h3>
 
- Begun to commenting source code
- Optimized xml/xsl for animations
- Added detection to screen borders
- Try the beta pet: Arcanoid (go under options and select BETA PET: Nr "7")
- Optimized the online pet editor
 
 
 <h3>Version 0.8 (beta)<sup>15 jan 2016</sup>:</h3>
 
- Added a certificate to the application (to increase reputation)<br />
- More "child" animations features<br />
- Solved some bugs with option and about box<br />
- A new webpage design!<br />
 
 
 <h3>Version 0.7 (beta)<sup>11 jan 2016</sup>:</h3>
 
- Added child animations, for bath/flowers and so on<br />
- Under option, you can select between 2 animations<br />
- Make your PET under <a href='pets/generator.php'>XML generator</a><br />
- Added a new animation to the sheep<br />
 
 
 <h3>Version 0.6.1 (beta)<sup>06 jan 2016</sup>:</h3>
 
- xml and xsd updated, to allow more functionality and more flexibility.<br />
- <a href='pets/generator.php'>XML generator</a>, to allow you to create your mate!
 
 
 <h3>Version 0.6 (beta)<sup>03 jan 2016</sup>:</h3>
 
- Run-animation was wrong (sheep running to the screen border)<br />
- New collaborator <b>Sergi</b> made some changes:<br />
- removed the mountain and replaced it with a tray icon<br />
- added an about box<br />
 
 
 <h3>Version 0.5 (beta)<sup>23 dec 2015</sup>:</h3>
 
- Rewrote completly the code, to be open source<br />
- <s>Sheep can take a bath</s><br />
- You can create your own animation!!!<br />
- Check the project page: <a href='https://github.com/Adrianotiger/desktopPet/'>GitHub</a><br />
- Check the manual page to create your own animation: <a href='https://github.com/Adrianotiger/desktopPet/wiki'>Wiki</a><br />
 
 
 <h3>Version 0.3 (beta)<sup>15 dec 2015</sup>:</h3>
 
- Better performance<br />
- Optimized window detection and detect if window is not anymore in foreground<br />
- Sheep can pee<br />
- Sheep can fall down from taskbar (reborn)<br />
- Sheep can take a bath<br />
 
 
 <h3>Version 0.2 (beta)<sup>10 dec 2015</sup>:</h3>
 
- Added "info dialog" functionality<br />
- Sheep can sleep in 2 different ways<br />
- Sheep falls down in a better way<br />
- Detect window on the desktop and walk on it<br />
- Hurt on the screen border<br />
- Kill all sheeps<br />
 
 
 <h3>Version 0.1 (beta):</h3>
 
- First version. Sheep can walk and run on the taskbar.
 
