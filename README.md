# AddIn
C# project using ArcPro SDK
I used this project to learn C# and ArcPro SDK, the original one was from **here** but it was written in ArcObject for ArcMap
Currently it has 5 add-ins:
1. Identify tool
2. Find nearest tower
3. Generate tower ranges
4. Measure strenth of signal
5. Generate dead areas

What to add:
2. Add toggling cabapility
3. Maybe enhancing the icons

## Notes for the gdb
1. If you want to change the name of the features/tables or their fields, make sure also change it in the code to avoid excpections
2. If you want to create your own classes, make sure to uncheck the Z-field unless it is a 3D feature
