# HDLethalCompany V1.5.1 - Sligili | CLIENT SIDE 🎈

*From now on updates will be less frequent*

## INSTALLATION 🛠
### Extract on the main game folder (where ```Lethal Company.exe``` is located). Make sure you have [BepInEx](https://github.com/BepInEx/BepInEx) installed

## CONFIGURATION ⚙
### Open ```BepInEx/config/HDLethalCompany.cfg``` with a text editor.

📃*This mod maintains the vanilla rendering aspect ratio ***to avoid breaking any HUD elements***.  
Calculate the value equivalent to your desired resolution -> ```DesiredResolutionWidth/860```*

- **RESOLUTION FIX:** Set ```EnableRes``` to ```true``` or ```false``` - if false, disables my custom resolution method so you can add another resolution mod with a different solution like [this one](https://www.nexusmods.com/lethalcompany/mods/8) or any WideScreen fix

- **RESOLUTION SCALE:** Set ```value``` to any number between ```1.000``` (*or lower*) - ```4.500``` (The UI scanned elements have slightly incorrect offsets after 3.000)
  - ***0.923*** -> ***800x480*** (*Increases performance*)
  - ***1.000*** -> ***860x520*** (*Vanilla resolution*)
  - ***1.488*** -> ***1280x720*** 
  - ***2.233*** -> ***1920x1080*** (*Default mod resolution*)
  - ***2.977*** -> ***2560x1440*** 
  - ***4.465*** -> ***3840x2060*** 

- **TEXTURE QUALITY:** Set ```TextureQuality``` to any of the following values ```0``` ```1``` ```2``` ```3``` - modifies texture resolution
  - ***0*** -> ***VERY LOW*** (*Eight resolution*)
  - ***1*** -> ***LOW*** (*Quarter resolution*)
  - ***2*** -> ***MEDIUM*** (*Half resolution*)
  - ***3*** -> ***HIGH*** (*Full resolution - Vanilla and default mod value*)

- **VOLUMETRIC FOG QUALITY:** Set ```FogQuality``` to any of the following values ```0``` ```1``` ```2``` ```3``` - modifies the volumetric fog budget
  - ***0*** -> ***VERY LOW*** (*Lower than the Vanilla preset, increases performance*)
  - ***1*** -> ***LOW*** (*Vanilla preset and the default mod value*)
  - ***2*** -> ***MEDIUM*** 
  - ***3*** -> ***HIGH*** (*Can really impact performance at high resolutions*)

- **LEVEL OF DETAIL:** Set ```LOD``` to any of the following values ```0``` ```1``` ```2``` - modifies the distance at which models decrease their poly count/disappear
  - ***0*** -> ***LOW*** (*half distance*)
  - ***1*** -> ***MEDIUM*** (*Vanilla distance - Default mod value*)
  - ***2*** -> ***HIGH*** (*Twice the distance*)

- **SHADOW QUALITY:** Set ```ShadowQuality``` to any of the following values ```0``` ```1``` ```2``` ```3``` - modifies the maximum resolution they can reach
  - ***0*** -> ***VERY LOW*** (*disables shadows*)
  - ***1*** -> ***LOW*** (*256 max resolution*)
  - ***2*** -> ***MEDIUM*** (*1024 max resolution*)
  - ***3*** -> ***HIGH*** (*2048 max resolution - Vanilla and default mod value*)

- **POST-PROCESSING:** Set ```EnablePP``` to ```true``` or ```false``` - if false, disables the HDRP Custom Pass, therefore disabling color grading 

- **VOLUMETRIC FOG:** Set ```EnableFOG``` to ```true``` or ```false``` - if false, disables the HDRP Volumetric Fog. **Use this as a last resource in case lowering the fog quality is not enough to get decent performance**

- **ANTI-ALIASING:** Set ```EnableAA``` to ```true``` or ```false``` - if true, enables built-in SMAA

- **FOLIAGE TOGGLE:** Set ```EnableF``` to ```true``` or ```false``` - if false, disables the "Foliage" layer (trees won't be affected, only most bushes and grass)

## CHANGELOG 🕗

- ***v1.5.1:***
     - Added a new parameter **Foliage**
     - Fog Quality won't be overridden by the weather system anymore
     - Fixed invisible stairs with LOD set to 0
     - Fixed some settings being reset after leaving a game and joining another
- ***v1.5.0:***
     - Added a new parameter **Resolution Fix**
     - Now spectator camera gets all settings applied. Ship cameras only apply lower settings.
     - Removed LC_API dependency because of bugs
     - Tweaked the icon
- ***v1.4.1:***
     - Mod has a [GitHub](https://github.com/Sligili/HDLethalCompany) now
     - New icon
- ***v1.4.0:***
     - Added two new parameters **Level Of Detail** and **Shadow Quality**
     - LC_API now required
     - Texture Quality instructions now included
- ***v1.3.0:***
     - The scanner should work now
     - Added a new parameter **Texture Quality** 
     - Changes now apply to the spectator camera aswell
     - The DLL has been renamed
- ***v1.2.0:***
     - SMAA actually works now (UwU)
     - Added a new parameter **Fog Quality** 
     - Now the resolution scale multiplier value can go up to ```4.500``` without breaking the scanner (too much)
- ***v1.1.2:*** 
     - Read Me formatting :P
- ***v1.1.1:*** 
     - Fixed some typos
