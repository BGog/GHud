GHud
=========================

Plugin for Kerbal Space Program (KSP) which adds displays to Logitech G series keyboards.  G19, G510 etc.


For Development
=========================

Make sure to put the files Assembly-CSharp.dll and UnityEngine.dll located in {KSP_Root}\KSP_Data\Managed\
into the KSPFiles folder. this will allow you to build straight away no matter what version of VS or hopefully
IDE's you want to use. the "testGHud" project in the solution is to test GHud without running KSP, it needs to
have the build mode set to "Debug" (As far as I know) and most likely a good idea to uncomment the #define TEST.
If you want to compile for use with KSP make sure to select the "Release" Profile else it will not compile
in a way that KSP can use