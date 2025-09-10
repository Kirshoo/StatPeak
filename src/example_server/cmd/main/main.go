package main

import (
	"example_server"
	"net/http"
	"time"
	"fmt"
)

func main() {
	fmt.Println("Running example server...")

	mux := http.NewServeMux()

	host := "localhost:5050"

	looksPath := "/looks"
	statsPath := "/stats"

	mux.HandleFunc(looksPath, example_server.HandleOutfitRequest)
	mux.HandleFunc(statsPath, example_server.HandleStatRequest)

	server := &http.Server{Addr: host, Handler: mux}
	go func() {
		_ = server.ListenAndServe()
	}()
	defer server.Close()

	fmt.Println("Ready and waiting for requests...")
	time.Sleep(1 * time.Minute)
}
