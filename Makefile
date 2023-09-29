default:
	@echo "Please specify a target to make."

clean:
	rm -rf src/frontend/dist
	rm -rf src/backend/wwwroot
	mkdir -p src/backend/wwwroot

copyui:
	cp -r src/frontend/dist/* src/backend/wwwroot

build-ui: clean
	@echo "Building frontend..."
	cd src/frontend && bun run build

run: build-ui
	@echo "Building frontend..."
	cd src/frontend && bun run build
	cp -r src/frontend/dist/* src/backend/wwwroot
	cd src/backend && dotnet watch run .