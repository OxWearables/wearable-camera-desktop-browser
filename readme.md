wearable-camera-desktop-browser
======================

A tool to help health researchers download, manage, and annotate wearable camera data.


<h2>Usage</h2>
This tool has a graphical user interface, see
(image:http://www.clarity-centre.org/sensecamwiki/images/thumb/8/8f/Dcu_sensecam_browser_screenshot.jpg/120px-Dcu_sensecam_browser_screenshot.jpg)

Key features:
* Compatible with following wearable cameras: Vicon Autographer, Vicon Revue 3MP, Vicon Revue, Microsoft SenseCam
* Multi-user functionality - useful for researchers managing data from multiple subjects
* Easy labelling/annotation of events
* Event segmentation - option to automatically summarise thousands of images into a small number of events e.g. 2,000 images on average segmented into ~20 events
* Platform for future research applications - backend database allows for easy extension of this browser for applications to suit your research purposes

[Click here for customised usage options on our wiki.]
(https://github.com/aidendoherty/biobankAcceleromerAnalysis/wiki/1. Usage)


<h2>Installation</h2>
[Click here for the installer]
(https://github.com/aidendoherty/biobankAcceleromerAnalysis/wiki/2. Installation)

If you would like to install the browser onto your computer you can follow the installation instructions in the "Downloads" section. If you have any problems installing this browser please use the "Discussions" section to explain your issue, which will be dealt with promptly. Note the browser only works on Windows PCs at the minute.

For software developers, the source code is intended for Visual Studio.

[Click here for detailed information on installing this software on our wiki.]
(https://github.com/aidendoherty/biobankAcceleromerAnalysis/wiki/2. Installation)

To compile the following is required:

Visual Studio C# 2010 (or newer)
WPFToolkit.Design.dll
WPFToolkit.dll
WPFToolkit.VisualStudio.Design.dll
Image-Unavailable.gif
adding photos.bmp
doherty_sensecam.db
System.Data.SQLite.dll
System.Data.SQLite.xml
delete_images_from_sensecam.exe


<h2>Under the hood</h2>
We are using a combination of published methods to organise wearable camera data
into meaningful episodes of health behaviours.


<h6>Licence</h6>
This project is released under a [BSD 2-Clause Licence](http://opensource.org/licenses/BSD-2-Clause) (see LICENCE file)


<h2>Citation to Use</h2>
Please cite this article:
[Doherty AR, Moulin CJ, Smeaton AF. Automatically assisting human memory: a SenseCam browser. Memory. 2011;19(7):785–95.]
(http://www.ncbi.nlm.nih.gov/pubmed/20845223)

Bibtex entry
@article{wearableCameraBrowser,
author = {Doherty, Aiden R. and Moulin, Chris J. A. and Smeaton, Alan F.},
title = {Automatically assisting human memory: A SenseCam browser},
journal = {Memory},
volume= {19},
number = {7},
pages = {785-795},
year = {2011},
doi = {10.1080/09658211.2010.509732},
URL = {http://www.tandfonline.com/doi/abs/10.1080/09658211.2010.509732},
eprint = {http://www.tandfonline.com/doi/pdf/10.1080/09658211.2010.509732}
}


<h6>Acknowledgements</h6>
Alan Smeaton, Cathal Gurrin, Niamh Caprani, Zhengwei Qiu, Dian Zhang [Dublin City University]
Charlie Foster, Gill Cowburn, Anne Matthews, Emma Thomas [University of Oxford]
Paul Kelly [University of Edinburgh]
Chris Moulin [NCRS Grenoble]
Jacqueline Kerr, Simon Marshall, Suneeta Godbole, Jacqueline Chen, Sam Fernald [University of California San Diego]
Melody Oliver [Auckland University of Technology]


