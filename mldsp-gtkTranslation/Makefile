
EXTRA_DIST =  Makefile


# Warning: This is an automatically generated file, do not edit!

srcdir=.
top_srcdir=..

include $(top_srcdir)/Makefile.include
include $(top_srcdir)/config.make

BUILD_DIR = .
INSTALL_DIR = $(DESTDIR)@expanded_libdir@/$(PACKAGE)

FILES = 

GMO_FILES = $(patsubst %.po,$(BUILD_DIR)/%.gmo,$(FILES))
MO_FILES = $(foreach po,$(FILES), $(INSTALL_DIR)/$(basename $(po))/LC_MESSAGES/$(PACKAGE).mo)

all: $(GMO_FILES)

$(GMO_FILES): $(BUILD_DIR)/%.gmo: %.po
	mkdir -p $(BUILD_DIR)
	msgfmt '$<' -o '$@'
	
$(MO_FILES): $(INSTALL_DIR)/%/LC_MESSAGES/$(PACKAGE).mo : $(BUILD_DIR)/%.gmo
	mkdir -p $(dir $@)
	cp '$<' '$@'

install-local: $(MO_FILES)

CLEANFILES = $(GMO_FILES)
install: install-local
uninstall: uninstall-local
clean: clean-local

include $(top_srcdir)/rules.make
