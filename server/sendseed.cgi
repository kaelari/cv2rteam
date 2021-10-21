#!/usr/bin/perl -w
use strict;
use warnings;
use CGI "param";
use lib "/usr/lib/cgi-bin";
use dbconnect;
our $dbh=db::connectdb();
$dbh->do("use cv2rteam");


my $seed=param("seed");
my $whip= param("whip");
my $items = param("items");
my $bodys = param("bodys");
my $keys = param("keys");
my $garlic = param("garlic");
my $laurels = param("laurels");
my $whip1=param("whip1");
$dbh->do("UPDATE `data` SET whip = ?, items = ?, keyitems = ?, bodies = ?, `garlic` = ?, `laurels` = ?, whip1 = ? WHERE `seed` = ?", undef, ($whip, $items, $keys, $bodys, $garlic, $laurels, $whip1,$seed));
print "Content-type: text/plain\n\n";
print "Success\r\n";
