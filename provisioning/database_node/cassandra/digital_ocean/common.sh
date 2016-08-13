#digital ocean install script
sudo su
mkfs.xfs /dev/sda
mkdir /shark
chmod 777 /shark 
mount /dev/sda /shark
mount | grep /dev/sda
chmod 777 /shark
mkdir /shark/data
mkdir /shark/cassandra

echo "/dev/sda                /shark         xfs	 defaults 0	 0">>/etc/fstab
