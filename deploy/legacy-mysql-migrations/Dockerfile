# Separate private service; only /var/lib/mysql is persisted by Render.
FROM mysql:8.0.46
CMD ["mysqld", "--innodb-buffer-pool-size=64M", "--performance-schema=OFF", "--max-connections=40"]
