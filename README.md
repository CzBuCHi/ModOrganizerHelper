# ModOrganizerHelper
ModOrganizerHelper

NMM dont have VFS, VFS in MO2 is buggy and Vortex is still somewhere on the horizont ...

This is just experimental piece of crap (just to proove mysel, it can be done).

How to use it:

1) install mods via MO2 as usually 
2) run LOOT, bodyslide from MO2
3) run game from MO2 to make sure everything works correcly.
4) after closing game and MO crash run this tool with single parameter: path to folder, where file 'ModOrganizer.ini' is 
(mo2 defaults to '%LOCALAPPDATA%\ModOrganizer\Fallout 4\', but due MO2 bug probably at 'c:\ModOrganizer2Data\Fallout 4\')
5) after tool finish its work game data folder will have all nesseseary files inside (more preciselly these files will be hard links to files inside 'c:\ModOrganizer2Data\Fallout 4\mods\*\ directory)

