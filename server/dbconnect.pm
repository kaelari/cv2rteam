#!/usr/bin/perl
use DBI;
package db;
sub connectdb {
	my $dbh;
	$host = "localhost";
	$dbuser = "";
	$dbpass = "";
	$dbname = "";
	$dbh = DBI-> connect("DBI:mysql:$dbname:$host","$dbuser","$dbpass", {RaiseError=> 1, AutoCommit => 1,}) or die ("ACK! Can't connect to db");
	return $dbh;
}

sub checkconnect {
	my $dbh=shift;
	if ($dbh->ping){
		return $dbh;
	}
	return connectdb();
}
1;
