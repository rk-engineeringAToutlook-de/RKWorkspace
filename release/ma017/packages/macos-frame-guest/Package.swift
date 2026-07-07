// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "MacPdfFrameGuest",
    platforms: [
        .macOS(.v13)
    ],
    products: [
        .executable(name: "MacPdfFrameGuest", targets: ["MacPdfFrameGuest"])
    ],
    targets: [
        .executableTarget(name: "MacPdfFrameGuest")
    ]
)
