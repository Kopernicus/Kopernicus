# Kopernicus Makefile
# Copyright 2016 Thomas

# We only support roslyn compiler at the moment
CS := csc
MONO_ASSEMBLIES := /usr/lib/mono/2.0-api

# Build config
MODE := DEBUG
PARAMS_DEBUG := /debug+ /debug:portable /define:DEBUG
PARAMS_RELEASE := /define:RELEASE

# Build Outputs
CURRENT_DIR := $(shell pwd)
PLUGIN_DIR := $(CURRENT_DIR)/Distribution/Development
RELEASE_DIR := $(CURRENT_DIR)/Distribution/Release
BUILD_DIR := $(RELEASE_DIR)/GameData/Kopernicus/Plugins
PARSER := $(PLUGIN_DIR)/Kopernicus.Parser.dll
ONDEMAND := $(PLUGIN_DIR)/Kopernicus.OnDemand.dll
COMPONENTS := $(PLUGIN_DIR)/Kopernicus.Components.dll
CORE := $(PLUGIN_DIR)/Kopernicus.dll

# Code paths
CORE_CODE := $(CURRENT_DIR)/Kopernicus/Kopernicus
COMPONENT_CODE := $(CURRENT_DIR)/Kopernicus/Kopernicus.Components
ONDEMAND_CODE := $(CURRENT_DIR)/Kopernicus/Kopernicus.OnDemand
PARSER_CODE := $(CURRENT_DIR)/Kopernicus/external/config-parser/src

# Assembly References
CORLIB := $(MONO_ASSEMBLIES)/mscorlib.dll,$(MONO_ASSEMBLIES)/System.dll,$(MONO_ASSEMBLIES)/System.Core.dll
REFS := $(CORLIB),Assembly-CSharp.dll,UnityEngine.dll,UnityEngine.UI.dll,ModularFlightIntegrator.dll

# Zip File
ZIP_NAME := Kopernicus-$(shell git describe --tags)-$(shell date "+%Y-%m-%d").zip

### BUILD TARGETS ###
all: debug
debug: PARAMS=$(PARAMS_DEBUG)
debug: plugin
release: PARAMS=$(PARAMS_RELEASE)
release: plugin
core: $(PARSER) $(ONDEMAND) $(COMPONENTS) $(CORE)
components: $(COMPONENTS)
ondemand: $(ONDEMAND)
parser: $(PARSER)
plugin: core copy_plugin_files
	cd $(RELEASE_DIR); zip -r $(ZIP_NAME) .
	
### LIBRARIES ###
$(CORE): generate_dirs
	$(CS) $(PARAMS) /out:$(CORE) /nostdlib+ /target:library /platform:anycpu /recurse:$(CORE_CODE)/*.cs /reference:$(REFS),$(COMPONENTS),$(ONDEMAND),$(PARSER)
$(COMPONENTS): generate_dirs
	$(CS) $(PARAMS) /out:$(COMPONENTS) /nostdlib+ /target:library /platform:anycpu /resource:$(COMPONENT_CODE)/Assets/WorldParticleCollider.unity3d,Kopernicus.Components.Assets.WorldParticleCollider.unity3d /recurse:$(COMPONENT_CODE)/*.cs /reference:$(REFS)
$(ONDEMAND): generate_dirs
	$(CS) $(PARAMS) /out:$(ONDEMAND) /nostdlib+ /target:library /platform:anycpu /unsafe+ /recurse:$(ONDEMAND_CODE)/*.cs /reference:$(REFS)
$(PARSER): generate_dirs
	$(CS) $(PARAMS) /out:$(PARSER) /nostdlib+ /target:library /platform:anycpu /unsafe+ /recurse:$(PARSER_CODE)/*.cs /reference:$(REFS)

### UTILS ###
generate_dirs:
	mkdir -p $(PLUGIN_DIR)
copy_plugin_files:
	cp $(PARSER) $(ONDEMAND) $(COMPONENTS) $(CORE) $(BUILD_DIR)
clean:
	rm -r $(PLUGIN_DIR)
	rm $(RELEASE_DIR)/$(ZIP_NAME)
	rm *.dll