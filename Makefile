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

infra:
	@echo "Deploy infrastructure."
	@echo "Make sure you are authorized with: az login"
	az group create -n rg-skmin-eus-poc -g eastus
	cd infra && az deployment group create --resource-group rg-skmin-eus-poc --template-file main.bicep
