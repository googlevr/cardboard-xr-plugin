# How to generate a new `GfxPluginCardboard.a`

Follow the [Download and build the demo app](https://developers.google.com/cardboard/develop/ios/quickstart#download_and_build_the_demo_app)
up to step 2 to open `Cardboard.xcworkspace` workspace in XCode. Select the
`sdk` module and then `Generic iOS Device` as the target device (or choose to
build for a specific phone if that suits your needs better). Build the `sdk`
module, copy the generated `.a` file and replace the `GfxPluginCardboard.a` file in
this folder. Similarly, do the same with the `sdk.bundle` folder and its
contents if necessary.

Finally, set `IOSurface;OpenGLES;GLKit;Metal;MetalKit;` as the iOS framework
dependencies in the library [meta file](https://github.com/googlevr/cardboard-xr-plugin/blob/master/Runtime/iOS/GfxPluginCardboard.a.meta#L77).
