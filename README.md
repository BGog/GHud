GHud
=========================

Plugin for Kerbal Space Program (KSP) which adds displays to Logitech G series keyboards.  G19, G510 etc.


For Development
=========================

Make sure to put the files Assembly-CSharp.dll and UnityEngine.dll located in {KSP_Root}\KSP_Data\Managed\
into the KSPFiles folder. this will allow you to build straight away no matter what version of VS (or hopefully any other
IDE's you want to use). the "testGHud" project in the solution is to test GHud without running KSP, it needs to
have the build mode set to "Debug" (As far as I know) and also for the #define TEST at the top to be uncommented.
If you want to compile for use with KSP make sure to select the "Release" Profile or the "Debug" Profile with the
#define TEST commented out else it wont produce a dll that KSP can use.

Dev Tips
------------
to have a better dev enviroment and to speed up testing I suggest going to TriggerAU's wonderful blog post
[An Adventure in Plugin Coding - 3. A Fast Dev Enviroment... I hope](http://forum.kerbalspaceprogram.com/entries/1253-An-Adventure-in-Plugin-Coding-3-A-Fast-Dev-Environment-I-hope)
He shows how to make KSP load quickly and automate the testing process (i.e. copy all files into KSP, start KSP and
autoload into a save and ship).