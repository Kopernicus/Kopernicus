# Kopernicus Makefile
# Copyright 2016 Thomas

# We only support roslyn compiler at the moment
CS := csc
MONO_ASSEMBLIES := /usr/lib/mono/2.0

# Build Outputs
CURRENT_DIR := $(shell pwd)
PLUGIN_DIR := $(CURRENT_DIR)/Distribution/Development
RELEASE_DIR := $(CURRENT_DIR)/Distribution/Release
BUILD_DIR := $(RELEASE_DIR)/GameData/Kopernicus/Plugins
ONDEMAND := $(PLUGIN_DIR)/Kopernicus.OnDemand.dll
COMPONENTS := $(PLUGIN_DIR)/Kopernicus.Components.dll
CORE := $(PLUGIN_DIR)/Kopernicus.dll

# Code paths
CORE_CODE := $(CURRENT_DIR)/Kopernicus/Kopernicus
COMPONENT_CODE := $(CURRENT_DIR)/Kopernicus/Kopernicus.Components
ONDEMAND_CODE := $(CURRENT_DIR)/Kopernicus/Kopernicus.OnDemand

# Assembly References
CORLIB := $(MONO_ASSEMBLIES)/mscorlib.dll,$(MONO_ASSEMBLIES)/System.dll,$(MONO_ASSEMBLIES)/System.Core.dll
REFS := $(CORLIB),Assembly-CSharp.dll,Assembly-CSharp-firstpass.dll,KSPUtil.dll,UnityEngine.dll,UnityEngine.UI.dll,ModularFlightIntegrator.dll

# Zip File
ZIP_NAME := Kopernicus-$(shell git describe --tags)-$(shell date "+%Y-%m-%d").zip

### BUILD TARGETS ###
all: plugin
core: $(ONDEMAND) $(COMPONENTS) $(CORE)
components: $(COMPONENTS)
ondemand: $(ONDEMAND)
plugin: core copy_plugin_files
	cd $(RELEASE_DIR); zip -r $(ZIP_NAME) .
	
### LIBRARIES ###
$(CORE): generate_dirs
	$(CS) /debug+ /debug:portable /define:DEBUG /out:$(CORE) /nostdlib+ /target:library /platform:anycpu /recurse:$(CORE_CODE)/*.cs /reference:$(REFS),$(COMPONENTS),$(ONDEMAND)
$(COMPONENTS): generate_dirs
	$(CS) /debug+ /debug:portable /define:DEBUG /out:$(COMPONENTS) /nostdlib+ /target:library /platform:anycpu /resource:$(COMPONENT_CODE)/Assets/WorldParticleCollider.unity3d,Kopernicus.Components.Assets.WorldParticleCollider.unity3d /recurse:$(COMPONENT_CODE)/*.cs /reference:$(REFS)
$(ONDEMAND): generate_dirs
	$(CS) /debug+ /debug:portable /define:DEBUG /out:$(ONDEMAND) /nostdlib+ /target:library /platform:anycpu /unsafe+ /recurse:$(ONDEMAND_CODE)/*.cs /reference:$(REFS)

### UTILS ###
generate_dirs:
	mkdir -p $(PLUGIN_DIR)
copy_plugin_files:
	cp $(ONDEMAND) $(COMPONENTS) $(CORE) $(BUILD_DIR)
clean:
	rm -r $(PLUGIN_DIR)
	rm $(RELEASE_DIR)/$(ZIP_NAME)
	rm *.dll