import * as React from 'react';
import { Provider } from 'mobx-react';
import { hot } from 'react-hot-loader';

const app = hot(module)(() => (
    <Provider>
        <div>Hello</div>
    </Provider>
));

export default app;
