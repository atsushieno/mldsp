
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/mldsp.clr.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

APP_XAML_SOURCE=App.xaml
PROCESSING_CORE_CLR_DLL_SOURCE=../build/Processing.Core.CLR.dll
PROCESSING_CORE_CLR_DLL_MDB_SOURCE=../build/Processing.Core.CLR.dll.mdb
MLDSP_CLR_DLL_MDB_SOURCE=bin/Debug/mldsp.clr.dll.mdb
MLDSP_CLR_DLL_MDB=$(BUILD_DIR)/mldsp.clr.dll.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/mldsp-clr.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

APP_XAML_SOURCE=App.xaml
PROCESSING_CORE_CLR_DLL_SOURCE=../build/Processing.Core.CLR.dll
PROCESSING_CORE_CLR_DLL_MDB_SOURCE=../build/Processing.Core.CLR.dll.mdb
MLDSP_CLR_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(APP_XAML) \
	$(PROCESSING_CORE_CLR_DLL) \
	$(PROCESSING_CORE_CLR_DLL_MDB) \
	$(MLDSP_CLR_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(MLDSP_CLR_PC)  


RESGEN=resgen2

APP_XAML = $(BUILD_DIR)/App.xaml
PROCESSING_CORE_CLR_DLL = $(BUILD_DIR)/Processing.Core.CLR.dll
PROCESSING_CORE_CLR_DLL_MDB = $(BUILD_DIR)/Processing.Core.CLR.dll.mdb
MLDSP_CLR_PC = $(BUILD_DIR)/mldsp.clr.pc

FILES = \
	MidiPlayer.cs \
	PortMidiPlayer.cs \
	SMF.cs \
	mldsp_app.cs \
	App.xaml.cs \
	PortMidiSharp.cs \
	MidiMachine.cs \
	mldsp_playerstatuspanel.cs \
	mldsp_playtimestatuspanel.cs \
	mldsp_visualitem.cs \
	mldsp_keyonmeterpanel.cs \
	mldsp_spectrumanalyzer.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	App.xaml \
	mldsp.clr.pc.in 

REFERENCES =  \
	System \
	System.Core \
	-pkg:moonlight-desktop-2.0

DLL_REFERENCES =  \
	../build/Processing.Core.CLR.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,APP_XAML))
$(eval $(call emit-deploy-target,PROCESSING_CORE_CLR_DLL))
$(eval $(call emit-deploy-target,PROCESSING_CORE_CLR_DLL_MDB))
$(eval $(call emit-deploy-wrapper,MLDSP_CLR_PC,mldsp.clr.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'


$(ASSEMBLY_MDB): $(ASSEMBLY)
$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	make pre-all-local-hook prefix=$(prefix)
	mkdir -p $(shell dirname $(ASSEMBLY))
	make $(CONFIG)_BeforeBuild
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	make $(CONFIG)_AfterBuild
	make post-all-local-hook prefix=$(prefix)

install-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-install-local-hook prefix=$(prefix)
	make install-satellite-assemblies prefix=$(prefix)
	mkdir -p '$(DESTDIR)$(libdir)/$(PACKAGE)'
	$(call cp,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(APP_XAML),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PROCESSING_CORE_CLR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PROCESSING_CORE_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MLDSP_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(libdir)/pkgconfig'
	$(call cp,$(MLDSP_CLR_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(APP_XAML),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PROCESSING_CORE_CLR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PROCESSING_CORE_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MLDSP_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MLDSP_CLR_PC),$(DESTDIR)$(libdir)/pkgconfig)
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
