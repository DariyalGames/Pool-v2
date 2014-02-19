This is a project where you can base and expand it in any way you like to build a complete and even unique billiard game. It doesn't strictly have to be an 8-ball like this project.

Important changes done in the project settings are the following two:

1) Added a new tag named "ball" that all balls have.

2) In the Physics Manager, the Max Angular Velocity is set to 500.



Propably the most important code line is in the whiteball.js line 56:

rigidbody.AddForceAtPosition(cuehelp.transform.right*cuestr,transform.position+Vector3(cuex*camz,cuey*1.5,-cuex*camx));

While a tremendus amount of time has been in balancing the physics and behaviour to seem as realistic as possible, you may alter things to you liking mainly by the code line above. Be aware though that things are relative to the physical materials. So keep that in mind.

Last but not least, please note that the size of the ICON balls, are put in the editor to be used in wide screen resolutions. Please change the size of the balls in the editor accordingly for use in other resolutions.

----CONTROLS----
-Mouse to look around.
-Lower the camera point of view to show up the vector.
-Hold ALT (or right click) and move mouse to reposition the cue.
-Mouse Scrollwheel to set strength.
-Left Click to hit.
-Hold "Z" to scale top camera.
----------------

((NOTE: If you get a warning that "The reference script on this Behaviour is missing" it's because of the Post-Effects. If you have those Assets and want them, then please import them, else delete those Components. I've left them in becuase they are setted up carefully for this scene and to save you time in case you need them))

Feel free to contact me for any questions.

Thanks,
Gavalakis Vaggelis
nuverian@creative-minds.gr
www.nuverian.net
