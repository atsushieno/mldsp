#NOTE: it is to build mldsp.xap for desktop

all:
	mdtool build mldsp.sln
	mkdir -p build
	cp mldsp/bin/Debug/App.xaml build
	cp mldsp/bin/Debug/Processing.Core.CLR.dll* build
	cp mldsp/bin/Debug/mldsp.clr.dll* build
	mxap --application-name=mldsp-clr build
	cp build/mldsp-clr.xap mldsp-gtk/bin/Debug

run:
	mono --debug mldsp-gtk/bin/Debug/mldsp-gtk.exe
