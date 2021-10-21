# cv2rteam
Co-op Support for CV2R

This syncs the inventory of two or more players playing CV2R allowing them to play the same seed together. All players should be using the same seed. Start the emulator and load the rom, then enter a username and a seedname in the co-op software. You should see a list of everyone who has joined the seed. This doesn't have to be the same as the random seed but it should be unique. Everyone should be in game before the first person gets an item. to avoid possible issues.

Server side is cgi. I use mod-perl to speed it up but it should run as is as a cgi script. Simple database save and lookup. code is in server

