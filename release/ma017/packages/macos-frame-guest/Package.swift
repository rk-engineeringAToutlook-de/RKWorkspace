// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "MacPdfFrameGuest",
    platforms: [
        .macOS(.v13)
    ],
    products: [
        .executable(name: "MacPdfFrameGuest", targets: ["MacPdfFrameGuest"]),
        .executable(name: "MacPdfPortalOwner", targets: ["MacPdfPortalOwner"])
    ],
    targets: [
        .executableTarget(name: "MacPdfFrameGuest"),
        .executableTarget(name: "MacPdfPortalOwner")
    ]
)
