
EXTRA_DIST =  Makefile

# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/config.make

ifeq ($(CONFIG),DEBUG)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/mldsp-gtk.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../mldsp/bin/Debug/mldsp.clr.dll
BUILD_DIR = bin/Debug

MLDSP_CLR_XAP_SOURCE=../build/mldsp.clr.xap
MLDSP_CLR_DLL_SOURCE=../mldsp/bin/Debug/mldsp.clr.dll
APP_XAML_SOURCE=../mldsp/App.xaml
PROCESSING_CORE_CLR_DLL_SOURCE=../build/Processing.Core.CLR.dll
PROCESSING_CORE_CLR_DLL_MDB_SOURCE=../build/Processing.Core.CLR.dll.mdb
MLDSP_CLR_DLL_MDB_SOURCE=../mldsp/bin/Debug/mldsp.clr.dll.mdb
MLDSP_CLR_DLL_MDB=$(BUILD_DIR)/mldsp.clr.dll.mdb
MLDSP_GTK_EXE_MDB_SOURCE=bin/Debug/mldsp-gtk.exe.mdb
MLDSP_GTK_EXE_MDB=$(BUILD_DIR)/mldsp-gtk.exe.mdb

endif

ifeq ($(CONFIG),RELEASE)
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/mldsp-gtk.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../mldsp/bin/Release/mldsp-clr.dll
BUILD_DIR = bin/Release

MLDSP_CLR_XAP_SOURCE=../build/mldsp.clr.xap
MLDSP_CLR_DLL_SOURCE=../mldsp/bin/Release/mldsp-clr.dll
APP_XAML_SOURCE=../mldsp/App.xaml
PROCESSING_CORE_CLR_DLL_SOURCE=../build/Processing.Core.CLR.dll
PROCESSING_CORE_CLR_DLL_MDB_SOURCE=../build/Processing.Core.CLR.dll.mdb
MLDSP_CLR_DLL_MDB=
MLDSP_GTK_EXE_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(MLDSP_CLR_XAP) \
	$(MLDSP_CLR_DLL) \
	$(APP_XAML) \
	$(PROCESSING_CORE_CLR_DLL) \
	$(PROCESSING_CORE_CLR_DLL_MDB) \
	$(MLDSP_CLR_DLL_MDB) \
	$(MLDSP_GTK_EXE_MDB)  

BINARIES = \
	$(MLDSP)  


RESGEN=resgen2

MLDSP_CLR_XAP = $(BUILD_DIR)/mldsp.clr.xap
MLDSP_CLR_DLL = $(BUILD_DIR)/mldsp-clr.dll
APP_XAML = $(BUILD_DIR)/App.xaml
PROCESSING_CORE_CLR_DLL = $(BUILD_DIR)/Processing.Core.CLR.dll
PROCESSING_CORE_CLR_DLL_MDB = $(BUILD_DIR)/Processing.Core.CLR.dll.mdb
MLDSP = $(BUILD_DIR)/mldsp

FILES = \
	gtk-gui/generated.cs \
	MainWindow.cs \
	gtk-gui/MainWindow.cs \
	Main.cs \
	AssemblyInfo.cs 

DATA_FILES = 

RESOURCES = \
	gtk-gui/gui.stetic 

EXTRAS = \
	../README \
	../build/mldsp.clr.xap \
	../update-xap \
	mldsp.in 

REFERENCES =  \
	System \
	Mono.Posix \
	-pkg:gtk-sharp-2.0 \
	-pkg:glib-sharp-2.0 \
	-pkg:glade-sharp-2.0 \
	-pkg:moonlight-gtk

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

#Targets
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES)  $(top_srcdir)/config.make

include $(top_srcdir)/Makefile.include
#include $(srcdir)/custom-hooks.make



$(eval $(call emit-deploy-target,MLDSP_CLR_XAP))
$(eval $(call emit-deploy-target,MLDSP_CLR_DLL))
$(eval $(call emit-deploy-target,APP_XAML))
$(eval $(call emit-deploy-target,PROCESSING_CORE_CLR_DLL))
$(eval $(call emit-deploy-target,PROCESSING_CORE_CLR_DLL_MDB))
$(eval $(call emit-deploy-target,MLDSP_CLR_DLL_MDB))
$(eval $(call emit-deploy-wrapper,MLDSP,mldsp,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

# Targets for Custom commands
DEBUG_AfterBuild:
	(cd /home/atsushi/svn/commons-music-prog/mldsp && ./update-xap )


$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
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
	$(call cp,$(MLDSP_CLR_XAP),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MLDSP_CLR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(APP_XAML),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PROCESSING_CORE_CLR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(PROCESSING_CORE_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MLDSP_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call cp,$(MLDSP_GTK_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	mkdir -p '$(DESTDIR)$(bindir)'
	$(call cp,$(MLDSP),$(DESTDIR)$(bindir))
	make post-install-local-hook prefix=$(prefix)

uninstall-local: $(ASSEMBLY) $(ASSEMBLY_MDB)
	make pre-uninstall-local-hook prefix=$(prefix)
	make uninstall-satellite-assemblies prefix=$(prefix)
	$(call rm,$(ASSEMBLY),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(ASSEMBLY_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MLDSP_CLR_XAP),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MLDSP_CLR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(APP_XAML),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PROCESSING_CORE_CLR_DLL),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(PROCESSING_CORE_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MLDSP_CLR_DLL_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MLDSP_GTK_EXE_MDB),$(DESTDIR)$(libdir)/$(PACKAGE))
	$(call rm,$(MLDSP),$(DESTDIR)$(bindir))
	make post-uninstall-local-hook prefix=$(prefix)

install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
