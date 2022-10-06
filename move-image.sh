#!/bin/sh
docker pull '||registry-source||/||image-source||:||version||' && \
docker tag '||registry-source||/||image-source||:||version||' '||registry-receiver||/||image-receiver||:||version||' && \
docker push '||registry-receiver||/||image-receiver||:||version||'