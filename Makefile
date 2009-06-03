#NOTE: it is to build mldsp.xap for desktop

all:
	mdtool build mldsp.sln
	mkdir -p build
	cp mldsp/bin/Debug/App.xaml build
	cp mldsp/bin/Debug/Processing.Core.CLR.dll* build
	cp mldsp/bin/Debug/mldsp.clr.dll* build
	#mxap --application-name=mldsp.clr build
	cd build; rm -f mldsp.clr.xap; zip mldsp.clr.xap AppManifest.xaml *.dll; cd ..
	cp build/mldsp.clr.xap mldsp-gtk/bin/Debug

run:
	cd mldsp-gtk/bin/Debug; mono --debug mldsp-gtk.exe $(DEVICE); cd ../../..
