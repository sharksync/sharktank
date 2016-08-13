#digital ocean install script
sudo su
yum clean all
dhclient
yum clean all
yum -y update
yum -y install epel-release wget
yum -y install nano

sudo systemctl start firewalld
sudo systemctl enable firewalld

sudo firewall-cmd --zone=public --add-service=ssh --permanent
sudo firewall-cmd --reload
yum -y install ftp://ftp.pbone.net/mirror/centos.karan.org/el5/extras/testing/i386/RPMS/jed-0.99.18-5.el5.kb.i386.rpm
