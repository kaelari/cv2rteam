#!/usr/bin/perl -w
use strict;
use warnings;
use CGI "param";
use lib "/usr/lib/cgi-bin";
use dbconnect;
our $dbh=db::connectdb();
$dbh->do("use cv2rteam");


my $seed=param("seed");
my $screenname = param("username");

unless ($seed){
    print "content-type: text/html\n\n";
    return;
}
my $data=$dbh->selectrow_hashref("SELECT * FROM `data` WHERE `seed` = ?", undef, ($seed));
if ($data){
    my $data2 = $dbh->selectall_arrayref("SELECT * FROM `players` WHERE `seed` = ?", {Slice=>{}}, ($seed));
    my @users;
    my $found = 0;
    foreach my $user (@{$data2}){
        if ($user->{username} eq $screenname){
            $found=1;
        }
        push (@users, "$user->{username}");
    }
    if ($found==0){
        $dbh->do("INSERT INTO `players`(`seed`, `username`) VALUES(?, ?)", undef, ($seed, $screenname));
    }
    my $userstring=join("-", @users);
    print "Content-type:text/plain\n\n";
    print "$data->{whip}, $data->{items}, $data->{keyitems}, $data->{bodies}, $data->{garlic}, $data->{laurels}, $data->{whip1}, $userstring\r\n";
}else {
    $dbh->do("INSERT INTO `players`(`seed`, `username`) VALUES(?, ?)", undef, ($seed, $screenname));
    $dbh->do("INSERT INTO `data` (`seed`, `whip`, `items`, `keyitems`, `bodies`, `garlic`, `laurels`, `whip1`) VALUES(?,?,?,?,?,?,?, ?)", undef, ( $seed, 0, 0, 0, 0, 0, 0, "00"));
    print "Content-type:text/plain\n\n";
    print "0,0,0,0,0,0,00, $screenname\r\n";
}



