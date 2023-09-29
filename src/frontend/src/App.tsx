import { For, createSignal } from 'solid-js'
import './App.css'

interface Message {
  prompt: string
  content: string
  usage: any
  deepContent: string
}

const mockMessages = [
  {
    prompt: 'What is your name?',
    content: 'My name is Solid',
    usage: 'const [name, setName] = createSignal("Solid")',
    deepContent: 'test'
  }
]

function App() {
  const [messages] = createSignal<Message[]>(mockMessages)

  const updateMessage = (idx: number, text: any) => {
    console.info(idx, text)
    // const item = {
    //   prompt: 'What is your name 2?',
    //   content: text,
    //   usage: 'const [name, setName] = createSignal("Solid")',
    //   deepContent: 'Solid is a declarative JavaScript library for creating user interfaces.'
    // }
    // const json = JSON.stringify(messages())
    // const newMessages = JSON.parse(json)
    // newMessages[idx].deepContent = item
    // setMessages(newMessages)
  }

  return (
    <>
      <h1>Messages</h1>
      <For each={messages()}>
        {(message) => (
          <div class="message">
            <div class="message__prompt">{message.prompt}</div>
            <div class="message__content">{message.content}</div>
            <div class="message__usage">{message.usage}</div>
            <div class="message__deep-content">{message.deepContent}</div>
            <button
              onClick={() => updateMessage(0, 'New content')}
            >Update</button>
          </div>
        )}
      </For>

    </>
  )
}

export default App
