#!/bin/bash

set -e

CLR_YELLOW_B="\033[1;33m"
CLR_YELLOW_E="\033[0m"
BUILD_DIR="./cs-parking-fees/bin/Debug/net472"

# These variables should be defined on local.envs (create manually if it not exists):
GAME_MODS_DIR=""

if [ -f local.envs ]; then
    echo "Loading local envs"
    source local.envs
fi

echo "============================================"
echo "Building Parking Fee Control Mod"
echo "Loaded ENVS:"
echo " - BUILD_DIR: $BUILD_DIR"
echo " - GAME_MODS_DIR: $GAME_MODS_DIR"
echo "============================================"

mkdir -p "$GAME_MODS_DIR"

# Build UI 
echo "............................................"
echo -e "${CLR_YELLOW_B}Building UI module... ${CLR_YELLOW_E}"
echo "............................................"
pushd cs-parking-fees/ui
if [ -f "package.json" ]; then
    
    if [ ! -d "node_modules" ]; then
        echo "Installing npm dependencies..."
        npm install
    fi
    
    npm run build
    echo "✓ UI build completed"
else
    echo "⚠ UI directory not found, skipping UI build"
fi

# Copy UI files if they exist
if [ -d "output/" ]; then
    echo "Copying UI files..."

    # For CS2, the UI files go directly in the mod folder root, not in ui subfolder
    # The game expects mod.json and the JS/CSS files in the same directory
    if [ -f "output/ParkingFeeControl.mjs" ]; then
        cp "output/ParkingFeeControl.mjs" "$GAME_MODS_DIR/"
    # elif [ -f "output/ParkingFeeControl.js" ]; then
    #     cp "output/ParkingFeeControl.js" "$GAME_MODS_DIR/"
    fi
    if [ -f "output/ParkingFeeControl.css" ]; then
        cp "output/ParkingFeeControl.css" "$GAME_MODS_DIR/"
    fi
    echo "✓ UI files copied"
else
    echo "⚠ UI build artifacts not found, skipping UI copy"
fi

# Copy mod.json for UI registration 
echo "Copying mod.json..."
cp mod.json "$GAME_MODS_DIR/"
echo "✓ mod.json copied"

popd

echo "............................................"
echo -e "${CLR_YELLOW_B}Compiling C# project...${CLR_YELLOW_E}"
echo "............................................"

dotnet build ParkingFeeControl.sln -c Debug

# Copy DLL
echo "Copying files to game mods folder..."
cp "$BUILD_DIR/ParkingFeeControl.dll" "$GAME_MODS_DIR/"

echo "............................................"
echo -e "${CLR_YELLOW_B}Copying external files${CLR_YELLOW_E}"
echo "............................................"

# Copy external locale JSON files 
pushd cs-parking-fees
if [ -d "Locale" ]; then
    echo "Copying locale files..."
    mkdir -p "$GAME_MODS_DIR/Locale"
    cp Locale/*.json "$GAME_MODS_DIR/Locale/" 2>/dev/null || true
    echo "✓ locale files copied"
else
    echo "⚠ Locale folder not found, skipping locale copy"
fi

popd

# Remove parking-config from destination only when requested
if [ "$1" == "clean" ]; then
    if [ -f "$GAME_MODS_DIR/parking-config.json" ]; then
        rm "$GAME_MODS_DIR/parking-config.json"
    fi
fi

echo "============================================"
echo "✓ Build completed successfully!"
echo "✓ Mod installed to: $GAME_MODS_DIR"
echo "============================================"