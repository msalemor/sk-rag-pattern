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

run: build-ui
	@echo "Building frontend..."	
	cp -r src/frontend/dist/* src/backend/wwwroot
	cd src/backend && dotnet watch run .

docker: build-ui
	@echo "Docker build"
	cd src/backend && docker build . -t am8850/skragminimal:dev

docker-run: docker
	@echo "Running the image"
	docker run --rm -p 8080:80 --env-file=.env am8850/skrag:dev
