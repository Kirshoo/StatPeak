package example_server

import (
	"encoding/json"
	"net/http"
	"fmt"
)

// TODO: register a domain to obtain partners API key 
// to create a better example
func ValidateTicket(ticket string) bool {
	return true;
}

type Stats struct {
	TotalJumpsStat float32 `json:"jumps"`
}

type Stats_DTO struct {
	Ticket string `json:"ticket"`
	Data Stats `json:"stats"`
}

func HandleStatRequest(w http.ResponseWriter, req *http.Request) {
	fmt.Println("Got stats request...")

	var statsBody Stats_DTO
	if err := json.NewDecoder(req.Body).Decode(&statsBody); err != nil {
		w.WriteHeader(http.StatusBadRequest)
		return;
	}

	if !ValidateTicket(statsBody.Ticket) {
		w.WriteHeader(http.StatusBadRequest)
		return;
	}

	w.WriteHeader(http.StatusOK)
	fmt.Printf("Received stats data: %+v\n", statsBody.Data)
	return;
}

type Looks struct {
	Color int `json:"color"`
	Hat int `json:"hat"`
}

type Looks_DTO struct {
	Ticket string `json:"ticket"`
	Data Looks `json:"looks"`
}

func HandleOutfitRequest(w http.ResponseWriter, req *http.Request) {
	fmt.Println("Got looks request...")

	var looksBody Looks_DTO
	if err := json.NewDecoder(req.Body).Decode(&looksBody); err != nil {
		w.WriteHeader(http.StatusBadRequest)
		return;
	}

	if !ValidateTicket(looksBody.Ticket) {
		w.WriteHeader(http.StatusBadRequest)
		return;
	}

	w.WriteHeader(http.StatusOK)
	fmt.Printf("Received looks data: %+v\n", looksBody.Data)
	return;
}
