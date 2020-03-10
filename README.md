# Unity Scene Picker

Load all project scenes in runtime - useful for testing and prototyping on mobile - single file to rule them all
  
If you want to buy me a beer  
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.me/wad1m)
  
## Screenshots

| Interface | Scene picker example | Active scene from list |
|------------|-------------|-------------|
| <img src="https://raw.githubusercontent.com/nukadelic/Unity-Scene-Picker/master/images/image1.png" width="400"> | <img src="https://raw.githubusercontent.com/nukadelic/Unity-Scene-Picker/master/images/image2.png" width="250"> | <img src="https://raw.githubusercontent.com/nukadelic/Unity-Scene-Picker/master/images/image3.png" width="250"> | 
| Includes a custom editor for the script | Mouse / touch based picker with scroll view | Press "M☰NU" button from any scene to return to scene selection view | 


## Installation 

Download or copy `SceneSelector.cs` and place it anywhere in the assets folder 

## Usage

1. Create an empty scene with no light or camera
2. Create empty game object and attach the script to it
3. Play / Build and run !

Optional : Attach a header image for the menu.

## How is it done ? 

Using IMGUI and screen dpi to adjust the UI scale and draw the buttons on the screen. A main scene will be chosen by this script as the entry point ( first scene in the build order ) - later it will fetch the list of all the build in scenes and one on a button touch / click. I included few tricks to handle scrolling on mobile since OnGUI ScrollView doesn't support mobile scroll. Once the script gets enabled it will fetch the list of all the scenes and will update them using the custom editor UI ( in unity inspector ) - those values are serialized hence its possible to retain anychanges during play mode. The header image is optional and will not take empty space if the header filed is left empty. On script start it will attach itself to the do not destroy scene and will be kept alive between all scenes. 

## Demo

In the screen shots above I cloned the Unity AR Foundation samples, created a new empty scene with only 1 empty game object and attached the screen to it. Using "Find and Add all scenes" and made sure all of them are enabled

On the 3rd image there is a preview of the back button that will be present in any loaded scene from the main menu.  

Fun fact : A red white blue flag is an international signalling code for `Tango` -> `Keep clear of me, I am engaged in pair trawling` -> `Pair trawling is a fishing activity carried out by two boats, with one towing each warp. As the mouth of the net is kept open by the lateral pull of the individual vessels, otter boards are not required` ( source : wikipedia ) , ( Flag colors was randomly generated from the AR Foundation sample scene )

## License

unlicense :: https://unlicense.org/

