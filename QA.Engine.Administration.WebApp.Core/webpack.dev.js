const webpack = require('webpack');
const merge = require('webpack-merge');
const common = require('./webpack.common');

module.exports = merge(common, {
    mode: 'development',
    devtool: 'source-map',
    plugins: [
        new webpack.HotModuleReplacementPlugin(),
    ],
    rules: [
        {
            test: /\.tsx?$/,
            exclude: /node_modules/,
            use: {
                loader: 'babel-loader'
            }
        },
        {
            test: /\.css$/,
            use: [
                { loader: 'style-loader' },
                { loader: 'css-loader' }
            ],
        },
    ],
    devServer: {
        hot: true,
        port: 3000,
        proxy: {
            '/api': 'http://localhost:3001',
        },
        open: true,
        openPage: '?backend_sid=c3386b2f-e098-4dfb-a794-e774cba9fcfc&site_id=52&param_name=site_id&customerCode=qa_demosite&actionCode=custom_635494192491212659&hostUID=fc4a5aa1-48b9-4a3a-84c1-bcd99a8a8ff3'
    }
});
