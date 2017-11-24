# Wrld Unity SDK Masking Demo

This is a small demo of using the [wrld Unity SDK](https://www.wrld3d.com/) to place the map on any surface you want. The SDK just provides rendering the whole map according to the camera's view. So you can't limit that to a specific size. 

This demo renders the map only inside the cube which acts like a stencil mask. The demo is heavily based on the excellent blog post from the wrld guys which used this method for AR. Read [part 1](https://www.wrld3d.com/blog/use-arkit-wrld-unity-sdk-part-1/) and [part 2](https://www.wrld3d.com/blog/using-arkit-wrld-sdk-part-2/) to understand what is done exactly.

If you move the map, it will automatically downloads the new map tiles and displays it. For using this demo you will need a valid SDK key from wrld, to get one just register at their website.

![alt-text](https://github.com/isenmann/WrldUnityMaskingDemo/blob/master/wrldDemo.gif "gif animation")

This demo includes the official [wrld Unity SDK from the asset store](https://www.assetstore.unity3d.com/en/#!/content/86284), have a look there for a newer version. The SDK was modified according to the changes mentioned in their blog posts.
