wget -c -O cassandra.tar.gz http://mirror.vorboss.net/apache/cassandra/3.7/apache-cassandra-3.7-bin.tar.gz
tar -xvzf cassandra.tar.gz -C /shark
mv /shark/apache-cassandra-3.7 /shark/cassandra
rm -rf cassandra.tar.gz
mkdir /shark/database/log
