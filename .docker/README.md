
```shell
docker-compose -f docker-compose.yml --env-file .env --env-file .env.local up -d --build
```

El stack publica la aplicacion en `http://api2.local/storage/blobstorage`.

Antes de probarlo desde el host, anade esta entrada al archivo `hosts` del sistema:

```text
127.0.0.1 api2.local
```