//import './App.css'

import { For, createSignal } from "solid-js"
import SolidMarkdown from "solid-markdown"

interface ISettings {
  max_tokens: string,
  temperature: string,
  limit: string,
  minRelevance: string
}

interface IMessage {
  query: string
  text: string
  usage: {
    completionTokens: number
    promptTokens: number
    totalTokens: number
  }
  citations: {
    collection: string
    fileName: string
  }[]
}

interface IQuery {
  collection: string
  query: string
  maxTokens: number
  temperature: number
  limit: number
  minRelevanceScore: number
}

const DefaultSettings: ISettings = {
  max_tokens: "2000",
  temperature: "0.3",
  limit: "3",
  minRelevance: "0.77"
}

function App() {
  const [settings, setSettings] = createSignal(DefaultSettings)
  const [query, setQuery] = createSignal("")
  const [conversation, setConversation] = createSignal<IMessage[]>([])

  const sendQuery = async () => {
    const payload: IQuery = {
      collection: "docs",
      query: query(),
      maxTokens: parseInt(settings().max_tokens),
      temperature: parseFloat(settings().temperature),
      limit: parseInt(settings().limit),
      minRelevanceScore: parseFloat(settings().minRelevance)
    }
    const res = await fetch("/api/gpt/query", {
      method: "POST",
      headers: { "Content-Type": "application/json", "Accept": "application/json" },
      body: JSON.stringify(payload)
    })
    const data = await res.json()
    console.info(JSON.stringify(data, null, 2))
    setConversation([...conversation(), {
      query: query(),
      text: data.text,
      usage: data.usage,
      citations: data.citations
    }])
    setQuery("")
  }

  return (
    <>
      <nav class='p-2 bg-blue-950 text-white font-semibold text-lg'>
        Search your documents
      </nav>
      <nav class='p-2 bg-blue-900 text-white font-semibold text-lg flex flex-row space-x-2'>
        <div class='space-x-2'>
          <label>Max Tokens:</label>
          <input type="text" class='px-1 w-16 text-black'
            value={settings().max_tokens}
            onInput={e => setSettings({ ...settings(), max_tokens: e.currentTarget.value })}
          />
        </div>
        <div class='space-x-2'>
          <label>Temperature:</label>
          <input type="text" class='px-1 w-16 text-black'
            value={settings().temperature}
            onInput={e => setSettings({ ...settings(), temperature: e.currentTarget.value })}
          />
        </div>
        <div class='space-x-2'>
          <label>Limit:</label>
          <input type="text" class='px-1 w-16 text-black'
            value={settings().limit}
            onInput={e => setSettings({ ...settings(), limit: e.currentTarget.value })}
          />
        </div>
        <div class='space-x-2'>
          <label>Relevance:</label>
          <input type="text" class='px-1 w-16 text-black'
            value={settings().minRelevance}
            onInput={e => setSettings({ ...settings(), minRelevance: e.currentTarget.value })}
          />
        </div>
      </nav>
      <main class="container mx-auto">
        <div class="p-3 flex flex-col w-full space-y-2">
          <div class="flex flex-row">
            <textarea class="p-2 border w-[calc(100%-70px)] mr-[5px]" rows={5}
              placeholder="What is your question?"
              value={query()}
              onInput={e => setQuery(e.currentTarget.value)}
            />
            <button class="p-2 bg-blue-700 hover:bg-blue-600 text-white text-sm font-semibold w-[75px] ml-auto"
              onClick={sendQuery}
            >Search</button>
          </div>
          {/* TODO: Iterate over the array */}
          <For each={conversation()}>
            {message => (
              <>
                <div class="p-2 bg-blue-200 w-[90%] rounded-md">{message.query}</div>
                <div class="p-2 bg-blue-300 w-[90%] rounded-md ml-auto">
                  <SolidMarkdown children={message.text} />
                  <hr />
                  <div class="flex flex-row flex-wrap justify-center">
                    <For each={message.citations}>
                      {citation => (
                        <div class="p-2 bg-blue-400 mr-2">
                          <a href={`/${citation.collection}/${citation.fileName}`}>{citation.fileName}</a>
                        </div>
                      )}
                    </For>
                  </div>
                </div>
              </>
            )}
          </For>
        </div>
      </main>
    </>
  )
}


export default App
