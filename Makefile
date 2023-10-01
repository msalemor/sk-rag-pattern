default:
	@echo "Available commands are: run, ingest, docker, docker-run"

clean:
	rm -rf src/frontend/dist
	rm -rf src/backend/wwwroot
	mkdir -p src/backend/wwwroot

ingest:
	@echo "Ingesting data..."
	cd src/ingestion && dotnet run .

build-ui: clean
	@echo "Building frontend..."
	cd src/frontend && bun run build
	cp -r src/frontend/dist/* src/backend/wwwroot/

run: build-ui
	@echo "Building frontend..."		
	cd src/backend && dotnet watch run .

docker: build-ui
	@echo "Docker build"
	cd src/backend && docker build . -t am8850/skragminimal:dev

docker-run: docker
	@echo "Running the image"
	cd src/backend && docker run --rm -p 8080:80 --env-file=.env am8850/skragminimal:dev

docker-push: docker
	@echo "Running the image"
	docker push am8850/skragminimal:dev

RG_NAME=rg-skragc-poc-eus
LOCATION=eastus
SKU=B1
PLAN_NAME=alemoskc-plan
APP_NAME=alemoskcapp
OS_TYPE=linux
infra: build-ui
	@echo "Deploy infrastructure."
	@echo "Make sure you are authorized with: az login"	
	az group create -g $(RG_NAME) -l $(LOCATION)
	az appservice plan create -g $(RG_NAME) -n $(PLAN_NAME) --sku $(SKU) --is-linux
	az webapp create --name $(APP_NAME) -g $(RG_NAME) --plan $(PLAN_NAME) --deployment-container-image-name am8850/skragminimal:dev

ENDPOINT=<>
GPT_DEPLOYMENT_NAME=<>
ADA_DEPLOYMENT_NAME=<>
GPT_API_KEY=<>
DB_PATH=<>
appconf:
	az webapp config appsettings set --name $(APP_NAME) -g $(RG_NAME) --settings GPT_DEPLOYMENT_NAME=$(GPT_DEPLOYMENT_NAME)
	az webapp config appsettings set --name $(APP_NAME) -g $(RG_NAME) --settings ADA_DEPLOYMENT_NAME=$(ADA_DEPLOYMENT_NAME)
	az webapp config appsettings set --name $(APP_NAME) -g $(RG_NAME) --settings GPT_ENDPOINT=$(ENDPOINT)
	az webapp config appsettings set --name $(APP_NAME) -g $(RG_NAME) --settings GPT_API_KEY=$(GPT_API_KEY)
	az webapp config appsettings set --name $(APP_NAME) -g $(RG_NAME) --settings DB_PATH=$(DB_PATH)

deploy: infra
	@echo "Deploying the app."
