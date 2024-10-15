docker container run -it --rm \
  --volume $PWD/src/Connextion.Web:/usr/src/app \
  -w /usr/src/app \
  node:lts /bin/bash -c "npx -y tailwindcss --watch -i ./wwwroot/app.css -o ./wwwroot/css/app.css"
