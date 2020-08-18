# Docker
## Quickstart local app development

To quickly start a local development environment for app testing purposes you can use of the docker-compose file:
```bash
# Solution root
cd docker
docker-compose up
``` 

To rebuild the image after a `git pull` run `docker-compose up --build`.

This will run the database, content api, mobile api and jobs api. Endpoints can be found here:

* Mobile API http://localhost:5004/
* Content API http://localhost:5005/
* BatchJobs API http://localhost:5006/

Swagger is available on all endpoints by appending `swagger` to the urls listed. For initial datebase setup open http://localhost:5006/swagger and execute the "nukeandpave" command.

## Running command line tools
To run command line tools, use `docker exec` to login to one of the running containers, e.g. `docker exec -ti docker_mobile_api_1 /bin/sh`. 
The `Tools` folder has all the available tools. For updating the app config for example, navigate to `Tools/DbFillExampleContent` and run `./DbFillExampleContent -a appconfig.json` an example config file is included in that directory, which can be edited for convenience.

**Please be aware of the 'quickstart development'-only semi-secrets in `docker/**/*`, the Docker configuration has no intentions to be used in publicly available testing, acceptation or production environments.**
